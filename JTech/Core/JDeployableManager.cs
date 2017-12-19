using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Core;

namespace Oxide.Plugins.JCore {

	public class JDeployableManager {

		public static Dictionary<Type, JInfoAttribute> DeployableTypes = new Dictionary<Type, JInfoAttribute>();
		public static Dictionary<Type, List<JRequirementAttribute>> DeployableTypeRequirements = new Dictionary<Type, List<JRequirementAttribute>>();

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

		// TODO
		// manage spawned deployables
		// distributive deployable update
		// load deployable types
		// load and spawn deployables from save file (async)
		// save deployables
		// clean up deployables on unload




	}
}
