using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Core;
using System.Reflection;

namespace Oxide.Plugins.JCore {

	public class JDeployableManager {

		// Deployables that are currently spawned
		private static Dictionary<int, JDeployable> spawnedDeployables = new Dictionary<int, JDeployable>();
		private static Dictionary<Type, List<JDeployable>> spawnedDeployablesByType = new Dictionary<Type, List<JDeployable>>();

		private static void SpawnedDeployablesAdd(int id, JDeployable instance, Type type) {
			spawnedDeployables.Add(id, instance);

			if (!spawnedDeployablesByType.ContainsKey(type))
				spawnedDeployablesByType.Add(type, new List<JDeployable>());
			spawnedDeployablesByType[type].Add(instance);
		}

		private static void SpawnedDeployablesRemove(int id, JDeployable instance) {

			spawnedDeployables.Remove(id);

			Type type;
			if (!TryGetType(instance.ToString(), out type))
				return;

			if (spawnedDeployablesByType.ContainsKey(type)) {
				spawnedDeployablesByType[type].Remove(instance);
			}
		}

		/// <summary>
		/// Get List of spawned deployables of type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<JDeployable> GetSpawned<T>() where T : JDeployable {

			List<JDeployable> spawned;
			spawnedDeployablesByType.TryGetValue(typeof(T), out spawned);
			return spawned;
		}

		/// <summary>
		/// Get Dictionary of all spawned deployables.  Key = Id and Value = JDeployable
		/// </summary>
		/// <returns></returns>
		public static Dictionary<int, JDeployable> GetSpawned() {
			return spawnedDeployables;
		}

		#region Update

		private static Dictionary<Type, int> CurrentUpdateTimeslot = new Dictionary<Type, int>();

		
		/// <summary>
		/// Distributed JDeployable Update
		/// </summary>
		public static void Update() {

			long now = DateTime.Now.Ticks;

			foreach (var deployablebytype in spawnedDeployablesByType) { // for each type of deployable
				if (deployablebytype.Value.Count > 0) {

					JUpdateAttribute updateinfo;
					if (DeployableTypeUpdates.TryGetValue(deployablebytype.Key, out updateinfo)) { // get update attribute for type

						// get timeslot for type
						int curtimeslot;
						if (!CurrentUpdateTimeslot.TryGetValue(deployablebytype.Key, out curtimeslot)) {
							CurrentUpdateTimeslot.Add(deployablebytype.Key, 0);
							curtimeslot = 0;
						}
						
						int updateDelay = updateinfo.updateDelay;

						// max concurrent updates
						// Max number of updates called at the same time for this deployable.  When exceeded, updateDelay is increased 
						if (updateinfo.maxConcurrentUpdates > 0) {
							double concurrent = (double) Math.Ceiling(((double) deployablebytype.Value.Count) / (updateDelay));
							if (concurrent > updateinfo.maxConcurrentUpdates) {
								updateDelay *= (int) Math.Ceiling(concurrent / updateinfo.maxConcurrentUpdates);
							}
						}

						//JInfoAttribute info;
						//DeployableTypes.TryGetValue(deployablebytype.Key, out info);
						//Interface.Oxide.LogInfo($"[JDeployableManager] --- {info.Name} timeslot {curtimeslot + 1} of {updateDelay} ---");

						// update deployables for current time slot
						for (int i = curtimeslot; i < deployablebytype.Value.Count; i += updateDelay) {
							JDeployable dep = deployablebytype.Value[i];
							
							if (dep.Update((now - dep._lastUpdate) * 0.0000001f)) { // convert ticks to seconds
								 
								//Interface.Oxide.LogInfo($"[JDeployableManager] {info.Name} {i+1} of {deployablebytype.Value.Count} updated with delta {(now - dep._lastUpdate) * 0.0000001f}s");

								dep._lastUpdate = now; // if true, set last update
							}
						}

						// set timeslot for next update
						if (curtimeslot + 1 < updateDelay) {
							CurrentUpdateTimeslot[deployablebytype.Key]++;
						} else {
							CurrentUpdateTimeslot[deployablebytype.Key] = 0;
						}

					}
				}
			}

		}

		#endregion

		#region Save and Load

		public class DeployableSaveData {
			public string t;
			public JDeployable.SaveData s;
		}

		public static void LoadDeployables() {
			if (DataManager.data == null || DataManager.data.d == null)
				return;

			int totalloadcount = 0;
			Dictionary<string, int> loadcount = new Dictionary<string, int>();
			
			foreach (var de in DataManager.data.d) {
				
				Type deployabletype;
				JInfoAttribute info;
				if (TryGetType(de.Value.t, out deployabletype) && DeployableTypes.TryGetValue(deployabletype, out info)) {

					if (!loadcount.Keys.Contains(info.Name))
						loadcount.Add(info.Name, 0);
					if (LoadJDeployable(de.Key, de.Value)) {
						loadcount[info.Name]++;
						totalloadcount++;
					} else
						Interface.Oxide.LogWarning($"[JCore] Failed to Load Deployable {de.Value} {de.Key}");
				}
			}

			string top = $"--- {totalloadcount} JDeployable(s) Loaded ---";
			Interface.Oxide.LogInfo($"[JCore] {top}");
			foreach (var count in loadcount)
				Interface.Oxide.LogInfo($"[JCore] > {count.Value} {count.Key}(s)");
			Interface.Oxide.LogInfo($"[JCore] {new String('-', top.Length)}");
		}

		private static bool LoadJDeployable(int id, DeployableSaveData data) {

			Type deployabletype;
			if (!TryGetType(data.t, out deployabletype))
				return false;

			// create instance of deployable
			var instance = Activator.CreateInstance(deployabletype);

			// apply save data to instance
			var savefield = deployabletype.GetField("data");
			if (savefield == null)
				return false;

			savefield.SetValue(instance, data.s);

			// spawn instance
			var methodInfo = deployabletype.GetMethod("Spawn");
			bool spawned = false;
			if (methodInfo != null) {
				try {
					spawned = (bool) methodInfo.Invoke(instance, null);
				} catch (Exception e) {
					spawned = false;
					Interface.Oxide.LogWarning($"[JCore] Failed to Spawn Deployable {e.InnerException.Message}");
				}
			}
			if (!spawned) {
				// clean up if deployable
				deployabletype.GetMethod("Kill")?.Invoke(instance, new object[] { BaseNetworkable.DestroyMode.None, false });
				return false;
			}

			// set Id
			var fieldInfo = deployabletype.GetField("Id");
			if (fieldInfo == null)
				return false;
			fieldInfo.SetValue(instance, id);

			// set last update
			var lastupdatefield = deployabletype.GetField("_lastUpdate");
			if (lastupdatefield == null)
				return false;
			lastupdatefield.SetValue(instance, DateTime.Now.Ticks);

			// add to spawnedDeployables
			SpawnedDeployablesAdd(id, (JDeployable) instance, deployabletype);
			
			return true;
		}

		public static void SaveJDeployables() {
			if (DataManager.data == null || DataManager.data.d == null)
				return;

			DataManager.data.d.Clear();

			int totalsavecount = 0;
			Dictionary<string, int> savecount = new Dictionary<string, int>();
			
			foreach (var deployablebytype in spawnedDeployablesByType) {

				JInfoAttribute info;
				if (DeployableTypes.TryGetValue(deployablebytype.Key, out info)) {

					savecount.Add(info.Name, 0);
					foreach (var de in deployablebytype.Value) {
						if (SaveJDeployable(de.Id, de)) {
							totalsavecount++;
							savecount[info.Name]++;
						} else
							Interface.Oxide.LogWarning($"[JCore] Failed to Save Deployable {de} {de.Id}");
					}
				}
			}

			string top = $"--- {totalsavecount} JDeployable(s) Saved ---";
			Interface.Oxide.LogInfo($"[JCore] {top}");
			foreach (var count in savecount) {
				if (count.Value > 0)
					Interface.Oxide.LogInfo($"[JCore] > {count.Value} {count.Key}(s)");
			}
			Interface.Oxide.LogInfo($"[JCore] {new String('-', top.Length)}");
			
		}

		private static bool SaveJDeployable(int id, JDeployable d) {

			DeployableSaveData sd = new DeployableSaveData();
			sd.t = d.ToString();
			sd.s = d.data;
			DataManager.data.d.Add(id, sd);

			return true;
		}

		public static void UnloadJDeployables() {
			foreach (var de in spawnedDeployables) {
				de.Value.Kill(BaseNetworkable.DestroyMode.None, false);
			}
			spawnedDeployables.Clear();
			spawnedDeployablesByType.Clear();
		}

		public static void UnloadJDeployable(int id) {
			JDeployable dep;
			if (spawnedDeployables.TryGetValue(id, out dep))
				UnloadJDeployable(dep);
		}

		public static void UnloadJDeployable(JDeployable dep) {
			dep.Kill();
		}

		public static void RemoveJDeployable(int id) {
			JDeployable dep;
			if (spawnedDeployables.TryGetValue(id, out dep)) {
				SpawnedDeployablesRemove(id, dep);
			}
		}

		#endregion

		#region Deployable Types

		public static Dictionary<Type, JInfoAttribute> DeployableTypes = new Dictionary<Type, JInfoAttribute>();
		public static Dictionary<Type, List<JRequirementAttribute>> DeployableTypeRequirements = new Dictionary<Type, List<JRequirementAttribute>>();
		public static Dictionary<Type, JUpdateAttribute> DeployableTypeUpdates = new Dictionary<Type, JUpdateAttribute>();

		/// <summary>
		/// JDeployable API
		/// Registers JDeployable to the JDeployableManager
		/// </summary>
		/// <typeparam name="T">JDeployable</typeparam>
		public static void RegisterJDeployable<T>() where T : JDeployable {

			// get info attribute
			JInfoAttribute info = (JInfoAttribute) System.Attribute.GetCustomAttribute(typeof(T), typeof(JInfoAttribute));

			if (info == null) {
				Interface.Oxide.LogWarning($"[JDeployableManager] Failed to register ({typeof(T)}) - Missing JInfoAttribute.");
				return;
			}

			if (DeployableTypes.ContainsKey(typeof(T)) || DeployableTypeRequirements.ContainsKey(typeof(T))) {
				Interface.Oxide.LogWarning($"[JDeployableManager] [{info.PluginInfo.Title}] {info.Name} has already been registered!");
				return;
			}

			// get requirements attributes
			List<JRequirementAttribute> requirements = System.Attribute.GetCustomAttributes(typeof(T), typeof(JRequirementAttribute)).OfType<JRequirementAttribute>().ToList();

			if (requirements == null || requirements.Count == 0) {
				Interface.Oxide.LogWarning($"[JDeployableManager] Failed to register ({typeof(T)}) - Missing JRequirementAttribute.");
				return;
			} else if (requirements.Count > 5) {
				Interface.Oxide.LogWarning($"[JDeployableManager] Failed to register ({typeof(T)}) - More than 5 JRequirementAttribute are not allowed.");
				return;
			}

			requirements.OrderBy(x => x.ItemId); // order requirements by their item id (just like the rust crafting menu)

			// get JUpdate attribute
			JUpdateAttribute jupdate = (JUpdateAttribute) System.Attribute.GetCustomAttribute(typeof(T), typeof(JUpdateAttribute));

			if (jupdate == null) {
				Interface.Oxide.LogWarning($"[JDeployableManager] Failed to register ({typeof(T)}) - Missing JUpdateAttribute.");
				return;
			} 

			DeployableTypes.Add(typeof(T), info);
			DeployableTypeRequirements.Add(typeof(T), requirements);
			DeployableTypeUpdates.Add(typeof(T), jupdate);
			if (!spawnedDeployablesByType.ContainsKey(typeof(T)))
				spawnedDeployablesByType.Add(typeof(T), new List<JDeployable>());

			Interface.Oxide.LogInfo($"[JDeployableManager] Registered Deployable: [{info.PluginInfo.Title}] {info.Name}");
			
		}

		/// <summary>
		/// JDeployable API
		/// Unregisters JDeployable from the JDeployableManager
		/// </summary>
		/// <typeparam name="T">JDeployable</typeparam>
		public static void UnregisterJDeployable<T>() where T : JDeployable {

			// get info attribute
			JInfoAttribute info = (JInfoAttribute) System.Attribute.GetCustomAttribute(typeof(T), typeof(JInfoAttribute));

			if (DeployableTypes.Remove(typeof(T)) && DeployableTypeRequirements.Remove(typeof(T)) && DeployableTypeUpdates.Remove(typeof(T))) {
				Interface.Oxide.LogInfo($"[JCore] Unregistered Deployable: [{info.PluginInfo.Title}] {info.Name}");
			} else {
				Interface.Oxide.LogInfo($"[JCore] Failed to Unregistered Deployable: [{info.PluginInfo.Title}] {info.Name}");
			}
		}

		public static bool TryGetType(string name, out Type deployabletype) {

			foreach (Type type in DeployableTypes.Keys)
				if (type.FullName == name) {
					deployabletype = type;
					return true;
				}

			deployabletype = null;
			return false;
		}

		#endregion

		#region Placing

		private static System.Random IDGenerator = new System.Random();
		private static int NewUID() {
			int id = (int) IDGenerator.Next(0, int.MaxValue);
			if (spawnedDeployables.ContainsKey(id))
				return NewUID();
			else
				return id;
		}

		public static bool PlaceDeployable(Type deployabletype, UserInfo userInfo) {

			var instance = Activator.CreateInstance(deployabletype);

			//var methodInfo = deployabletype.GetMethod("Place");
			//if (!(methodInfo != null && (bool) methodInfo.Invoke(instance, new object[] { userInfo })))
			//	return false;

			// place instance
			var methodInfo = deployabletype.GetMethod("Place");
			bool placed = false;
			if (methodInfo != null) {
				try {
					placed = ((bool) methodInfo.Invoke(instance, new object[] { userInfo }));
				} catch (Exception e) {
					placed = false;
					Interface.Oxide.LogWarning($"[JCore] Failed to Place Deployable {e.InnerException.Message}");
					userInfo.ShowErrorMessage("Failed to Place Deployable", e.InnerException.Message);
				}
			}
			if (!placed) {
				// clean up if deployable spawned anything
				userInfo.ShowErrorMessage("Failed to Spawn Deployable");
				deployabletype.GetMethod("Kill")?.Invoke(instance, new object[] { BaseNetworkable.DestroyMode.None, false });
				return false;
			}

			// create id
			var fieldInfo = deployabletype.GetField("Id");
			if (fieldInfo == null)
				return false;
			int id = NewUID();
			fieldInfo.SetValue(instance, id);

			// set last update
			var lastupdatefield = deployabletype.GetField("_lastUpdate");
			if (lastupdatefield == null)
				return false;
			lastupdatefield.SetValue(instance, DateTime.Now.Ticks);

			SpawnedDeployablesAdd(id, (JDeployable) instance, deployabletype);
			
			return true;
		}

		#endregion
	}
}
