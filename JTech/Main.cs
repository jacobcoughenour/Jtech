﻿using System;
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

	//PM.INSERT(PluginInfo)
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

			RegisterDeployables();
		}
		
		void Unload() {

			// TODO
			// save deployables
			// unload deployables

			// Destroy UserInfo from all the players
			var users = UnityEngine.Object.FindObjectsOfType<UserInfo>();
			if (users != null) {
				foreach (UserInfo go in users) {
					go.DestroyCui();
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

		void RegisterDeployables() {
			JDeployableManager.RegisterJDeployable<JTechDeployables.TransportPipe>();
			JDeployableManager.RegisterJDeployable<JTechDeployables.Assembler>();
			//JDeployableManager.RegisterJDeployable<JTechDeployables.TrashCan>();
			//JDeployableManager.RegisterJDeployable<JTechDeployables.AutoFarm>();
		}


		#region Player

		void OnPlayerSleepEnded(BasePlayer player) {
			// Add UserInfo to player
			UserInfo.Get(player);
		}

		#endregion

		#region Structure

		void OnHammerHit(BasePlayer player, HitInfo hit) {
			// TODO
			// open menu if deployable

			UserInfo.OnHammerHit(player, hit);

			//PM.DEBUGSTART
			ListComponentsDebug(player, hit.HitEntity);
			//PM.DEBUGEND
		}

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

		[ConsoleCommand("jtech.startplacing")]
		private void startplacing(ConsoleSystem.Arg arg) {

			if (arg.HasArgs()) {

				Type deployabletype;
				if (JDeployableManager.TryGetType(arg.Args[0], out deployabletype)) {
					
					UserInfo.StartPlacing(arg.Player(), deployabletype);
				}
			}
		}

		//PM.DEBUGSTART

		// Lists the ent's components and variables to player's chat

		void ListComponentsDebug(BasePlayer player, BaseEntity ent) {

			List<string> lines = new List<string>();
			string s = "<color=#80c5ff>───────────────────────</color>";
			int limit = 1030;

			foreach (var c in ent.GetComponents<Component>()) {

				List<string> types = new List<string>();
				List<string> names = new List<string>();
				List<string> values = new List<string>();
				int typelength = 0;

				foreach (FieldInfo fi in c.GetType().GetFields()) {

					System.Object obj = (System.Object) c;
					string ts = fi.FieldType.Name;
					if (ts.Length > typelength)
						typelength = ts.Length;

					types.Add(ts);
					names.Add(fi.Name);

					var val = fi.GetValue(obj);
					if (val != null)
						values.Add(val.ToString());
					else
						values.Add("null");

				}

				if (s.Length > 0)
					s += "\n";
				s += types.Count > 0 ? "╔" : "═";
				s += $" {c.GetType()} : {c.GetType().BaseType}";
				//s += " <" + c.name + ">\n";

				for (int i = 0; i < types.Count; i++) {

					string ns = $"<color=#80c5ff> {types[i]}</color> {names[i]} = <color=#00ff00>{values[i]}</color>";

					if (s.Length + ns.Length >= limit) {
						lines.Add(s);
						s = "║" + ns;
					} else {
						s += "\n║" + ns;
					}
				}

				if (types.Count > 0) {
					s += "\n╚══";
					lines.Add(s);
					s = string.Empty;
				}
			}

			lines.Add(s);

			foreach (string ls in lines)
				PrintToChat(player, ls);

		}

		//PM.DEBUGEND
	}
}
