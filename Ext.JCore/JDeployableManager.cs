using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Core;

namespace Oxide.Ext.JCore {

	public class JDeployableManager {

		private static HashSet<Type> DeployableTypes = new HashSet<Type>();
		public static bool UnregisterType<T>() where T : JDeployable => DeployableTypes.Remove(typeof(T));
		
		/// <summary>
		/// JDeployable API
		/// Registers Custom JDeployable to the JDeployableManager
		/// </summary>
		/// <typeparam name="T">Custom Deployable</typeparam>
		public static void RegisterJDeployable<T>() where T : JDeployable {

			// get info attribute
			JInfoAttribute info = (JInfoAttribute) System.Attribute.GetCustomAttribute(typeof(T), typeof(JInfoAttribute));

			if (info != null) {
				if (DeployableTypes.Add(typeof(T)))
					Interface.Oxide.LogInfo($"[JCore] Registered Custom Deployable: [{info.PluginInfo.Title}] {info.Name}");
				else
					Interface.Oxide.LogWarning($"[JCore] ([{info.PluginInfo.Title}] {info.Name}) has already been registered!");
				
			} else
				Interface.Oxide.LogWarning($"[JCore] Failed to register ({typeof(T)}) for Missing JInfo Attribute");

			Interface.Oxide.CallHook("OnRegisterJDeployable", info);
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
