using System;
using System.Collections;
using System.Collections.Generic;
using Oxide.Core.Plugins;
using Oxide.Plugins.JTechCore;
using UnityEngine;
using System.Linq;
using System.Text;

namespace Oxide.Plugins {

    [Info("JTech", "TheGreatJ", "1.0.0", ResourceId = 2402)]
    class JTech : RustPlugin {

        [PluginReference]
        private Plugin FurnaceSplitter;

		#region Oxide Hooks

		void Init() {

            Puts("hi");
			// TODO
			// register lang messages
			// load config
			// load commands
		}

		void OnServerInitialized() {

			// TODO
			// load save data
			// load deployables from save data
			// Put loaded message
		}

		void OnLoaded() {
			// TODO
			// register permissions
		}

		void Unload() {

			// TODO
			// destroy cui for each user
			// save deployables
			// unload deployables
		}

		void OnNewSave(string filename) {
			// TODO
			// clear save data
		}

		void OnServerSave() {
			// TODO
			// save deployables
		}


		#region Player

		void OnPlayerInit(BasePlayer player) {
			// TODO
			// register user
		}

		void OnPlayerDisconnected(BasePlayer player) {
			// TODO
			// unregister user
		}

		#endregion

		#region Structure

		void OnHammerHit(BasePlayer player, HitInfo hit) {
			// TODO
			// open menu if deployable
		}

		#endregion


		#endregion

	}
}

namespace Oxide.Plugins.JTechCore {

	public class BaseDeployable {

		// TODO
		// handle entity hooks (health, damage, repair)
		// owner, parent
		// cui menu
		// spawn/destroy
		// load/save


		public BaseDeployable() { }

		public bool Spawn(out string error) {
			error = string.Empty;


			return true;
		}

	}

	// component added to BaseEntity(s) for handling hooks
	public class BaseDeployableChild : MonoBehaviour {
		// TODO
	}

	// component added to parent for handling hooks and update
	public class BaseDeployableBehaviour : BaseDeployableChild {
		// TODO
	}

}

namespace Oxide.Plugins.JTechCore {

	public static class Data {

		public static void Load() {
			// TODO
		}

		public static void Save() {
			// TODO
		}

		private static void LoadData<T>(ref T data) => data = Core.Interface.Oxide.DataFileSystem.ReadObject<T>("JTech");
		private static void SaveData<T>(T data) => Core.Interface.Oxide.DataFileSystem.WriteObject("JTech", data);
	}
}

namespace Oxide.Plugins.JTechCore {

	public class DeployableManager {
		
		private Dictionary<ulong, BaseDeployable> registeredDeployables = new Dictionary<ulong, BaseDeployable>();

		// TODO
		// manage spawned deployables
		// load deployable types
		// load and spawn deployables from save file
		// save deployables
		// clean up deployables on unload

	}
}

namespace Oxide.Plugins.JTechCore.Deployables {

	public class Pipe : BaseDeployable {

		// TODO

	}
}
