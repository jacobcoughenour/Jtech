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

		// TODO
		// manage spawned deployables
		// distributive deployable update
		// load deployable types
		// load and spawn deployables from save file (async)
		// save deployables
		// clean up deployables on unload

		public class DeployableSaveData {
			public string t;
			public JDeployable.SaveData s;
		}

		public static Dictionary<Type, JInfoAttribute> DeployableTypes = new Dictionary<Type, JInfoAttribute>();
		public static Dictionary<Type, List<JRequirementAttribute>> DeployableTypeRequirements = new Dictionary<Type, List<JRequirementAttribute>>();

		// Deployables that are currently spawned
		public static Dictionary<int, JDeployable> spawnedDeployables = new Dictionary<int, JDeployable>();
		public static Dictionary<Type, List<JDeployable>> spawnedDeployablesByType = new Dictionary<Type, List<JDeployable>>();

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

		public static void LoadDeployables() {
			if (DataManager.data == null || DataManager.data.d == null)
				return;

			int loadcount = 0;
			
			foreach (var de in DataManager.data.d) {
				if (LoadJDeployable(de.Key, de.Value))
					loadcount++;
				else
					Interface.Oxide.LogWarning($"[JCore] Failed to Load Deployable {de.Value} {de.Key}");
			}

			Interface.Oxide.LogInfo($"[JCore] {loadcount} JDeployables Loaded");
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
					Interface.Oxide.LogWarning(e.InnerException.Message);
				}
			}
			if (spawned == false)
				return false;

			// set Id
			var fieldInfo = deployabletype.GetField("Id");
			if (fieldInfo == null)
				return false;

			fieldInfo.SetValue(instance, id);

			// add to spawnedDeployables
			SpawnedDeployablesAdd(id, (JDeployable) instance, deployabletype);
			
			return true;
		}

		public static void SaveJDeployables() {
			if (DataManager.data == null || DataManager.data.d == null)
				return;

			DataManager.data.d.Clear();

			int savecount = 0;

			foreach (var de in spawnedDeployables) {
				if (SaveJDeployable(de.Key, de.Value))
					savecount++;
				else
					Interface.Oxide.LogWarning($"[JCore] Failed to Save Deployable {de.Value} {de.Key}");
				
			}

			Interface.Oxide.LogInfo($"[JCore] {savecount} JDeployables Saved");
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
			
			DeployableTypes.Add(typeof(T), info);
			DeployableTypeRequirements.Add(typeof(T), requirements);
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

			if (DeployableTypes.Remove(typeof(T)) && DeployableTypeRequirements.Remove(typeof(T))) {
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

			var methodInfo = deployabletype.GetMethod("Place");
			if (!(methodInfo != null && (bool) methodInfo.Invoke(instance, new object[] { userInfo })))
				return false;

			var fieldInfo = deployabletype.GetField("Id");
			if (fieldInfo == null)
				return false;

			int id = NewUID();
			fieldInfo.SetValue(instance, id);
			
			spawnedDeployables.Add(id, (JDeployable) instance);
			
			return true;
		}

		

	}
}
