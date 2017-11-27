using System;
using System.Collections;
using System.Collections.Generic;
using Oxide.Core.Plugins;
using Oxide.Plugins.JTechCore;

namespace Oxide.Plugins {

    [Info("JTech", "TheGreatJ", "1.0.0", ResourceId = 2402)]
    class JTech : RustPlugin {

        [PluginReference]
        private Plugin FurnaceSplitter;

		#region Oxide Hooks

		void Init() {

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