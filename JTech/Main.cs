using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Oxide.Core.Plugins;
using Oxide.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;
using Oxide.Plugins.JCore;

namespace Oxide.Plugins {

    [Info("JTech", "TheGreatJ", "1.0.0", ResourceId = 2402)]
    class JTech : RustPlugin {

		#region Oxide Hooks

		void Init() {
			
			// TODO
			// register lang messages
			// load config
			// load commands

			NextFrame(() => {
				foreach (var player in BasePlayer.activePlayerList)
					UserInfo.Get(player);
			});

		}
		
		void OnServerInitialized() {

			// TODO
			// load save data
			// load deployables from save data
			// Put loaded message

			JDeployableManager.RegisterJDeployable<JTechDeployables.TransportPipe>();
			JDeployableManager.RegisterJDeployable<JTechDeployables.Assembler>();
		}

		

		void Unload() {

			// TODO
			// save deployables
			// unload deployables

			// Destroy UserInfo from all the players
			var users = UnityEngine.Object.FindObjectsOfType<UserInfo>();
			if (users != null) {
				foreach (UserInfo go in users) {
					go.HideOverlay();
					GameObject.Destroy(go);
				}
			}

		}

		// removes anything named UserInfo from the player
		//[ConsoleCommand("jtech.clean")]
		//private void cmdpipechangedir(ConsoleSystem.Arg arg) {

		//	List<UnityEngine.Object> uis = new List<UnityEngine.Object>();
		//	foreach (var player in BasePlayer.activePlayerList) {
		//		foreach (var c in player.GetComponents<Component>()) {
		//			if (c.GetType().ToString() == "Oxide.Plugins.JTechCore.UserInfo") {
		//				uis.Add(c);
		//			}
		//		}
		//	}

		//	foreach (var u in uis) {
		//		UnityEngine.Object.Destroy(u);
		//	}

		//	Puts($"{uis.Count} destroyed");

		//	NextFrame(() => {
		//		foreach (var player in BasePlayer.activePlayerList)
		//			UserInfo.Get(player);
		//	});
		//}

		void OnNewSave(string filename) {
			// TODO
			// clear save data
		}

		void OnServerSave() {
			// TODO
			// save deployables
		}


		#region Player

		void OnPlayerSleepEnded(BasePlayer player) {
			// Add UserInfo to player
			UserInfo.Get(player);
		}

		#endregion

		#region Structure

		//void OnHammerHit(BasePlayer player, HitInfo hit) {
		//	// TODO
		//	// open menu if deployable
		//}

		#endregion


		#endregion

		[ChatCommand("jtech")]
		private void jtechmainchat(BasePlayer player, string cmd, string[] args) {
			UserInfo.ShowOverlay(player);
		}

		[ConsoleCommand("jtech.showoverlay")]
		private void showoverlay(ConsoleSystem.Arg arg) {
			UserInfo.ShowOverlay(arg.Player());
		}

		[ConsoleCommand("jtech.closeoverlay")]
		private void closeoverlay(ConsoleSystem.Arg arg) {
			UserInfo.HideOverlay(arg.Player());
		}

	}
}
