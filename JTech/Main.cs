using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Oxide.Core.Plugins;
using Oxide.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;
using Oxide.Plugins.JTechCore;

namespace Oxide.Plugins {

	//PM.INSERT(PluginInfo, TheGreatJ, http://oxidemod.org/plugins/jpipes.2402/, https://github.com/jacobcoughenour/JTech)
	class JTech : RustPlugin {

		#region Oxide Hooks
		
		void Init() {

			// TODO
			// register lang messages
			// load config

			NextFrame(() => {
				foreach (var player in BasePlayer.activePlayerList)
					UserInfo.Get(player);
			});

			
		}

		void OnServerInitialized() {
			
			RegisterDeployables();

			DataManager.Load();
			JDeployableManager.LoadDeployables();

			// start update
			timer.Repeat(0.25f, 0, JDeployableManager.Update);
		}
		
		void Unload() {

			OnServerSave();
			JDeployableManager.UnloadJDeployables();

			// Destroy UserInfo from all the players
			var users = UnityEngine.Object.FindObjectsOfType<UserInfo>();
			if (users != null) {
				foreach (UserInfo go in users) {
					go.DestroyCui();
					go.CancelPlacing();
					GameObject.Destroy(go);
				}
			}

		}
		
		// removes anything named UserInfo from the player
		[ConsoleCommand("jtech.clean")]
		private void cmdjtechclean(ConsoleSystem.Arg arg) {

			List<UnityEngine.Object> uis = new List<UnityEngine.Object>();
			foreach (var player in BasePlayer.activePlayerList) {
				foreach (var c in player.GetComponents<Component>()) {
					if (c.GetType().ToString() == "Oxide.Plugins.JTechCore.UserInfo") {
						uis.Add(c);
					}
				}
			}

			foreach (var u in uis) {
				UnityEngine.Object.Destroy(u);
			}

			Puts($"{uis.Count} destroyed");

			NextFrame(() => {
				foreach (var player in BasePlayer.activePlayerList)
					UserInfo.Get(player);
			});
		}

		void OnNewSave(string filename) {
			// TODO
			// clear save data
		}

		void OnServerSave() {

			JDeployableManager.SaveJDeployables();
			DataManager.Save();
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

		void OnItemDeployed(Deployer deployer, BaseEntity entity) => UserInfo.Get(deployer?.GetOwnerPlayer())?.OnDeployPlaceholder(entity);

		void OnEntityBuilt(Planner planner, GameObject go) {
			BaseEntity entity = go?.GetComponent<BaseEntity>();
			if (entity != null)
				UserInfo.Get(planner?.GetOwnerPlayer())?.OnDeployPlaceholder(entity);
		}

		bool? CanMoveItem(Item item, PlayerInventory playerLoot, uint targetContainer, int targetSlot) {
			return UserInfo.Get(playerLoot.GetComponent<BasePlayer>())?.CanMoveItem(item, targetSlot);
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

		void OnStructureDemolish(BaseCombatEntity entity, BasePlayer player) => OnKilledChild((BaseEntity) entity);
		void OnEntityDeath(BaseCombatEntity entity, HitInfo info) => OnKilledChild((BaseEntity) entity);
		void OnEntityKill(BaseNetworkable entity) => OnKilledChild((BaseEntity) entity);

		void OnKilledChild(BaseEntity entity) {
			JDeployable.Child c = entity?.GetComponent<JDeployable.Child>();
			if (c != null && c.parent != null)
				KillDeployable(c.parent);
		}

		void KillDeployable(JDeployable deployable) {
			NextFrame(() => {
				deployable.Kill(BaseNetworkable.DestroyMode.Gib);
			});
		}

		bool? OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hitInfo) {

			if (entity != null && hitInfo != null) {

				JDeployable.Child c = entity?.GetComponent<JDeployable.Child>();
				if (c != null && c.parent != null) {
					
					float damage = hitInfo.damageTypes.Total();
					if (damage > 0) {
						
						float newhealth = entity.health - damage;
						if (newhealth > 0f)
							c.parent.SetHealth(newhealth);
						else
							KillDeployable(c.parent);
					}
					return true;
				}
			}
			return null;
		}

		bool? OnStructureUpgrade(BaseCombatEntity entity, BasePlayer player, BuildingGrade.Enum grade) {
			JDeployable.Child c = entity?.GetComponent<JDeployable.Child>();
			if (c != null && c.parent != null && player != null)
				return c.parent.OnStructureUpgrade(c, player, grade);
			return null;
		}

		void OnStructureRepair(BaseCombatEntity entity, BasePlayer player) {
			JDeployable.Child c = entity?.GetComponent<JDeployable.Child>();
			if (c != null && c.parent != null && player != null)
				NextTick(() => c.parent.OnStructureRepair(entity, player));
		}

		bool? CanPickupEntity(BaseCombatEntity entity, BasePlayer player) {
			JDeployable.Child c = entity?.GetComponent<JDeployable.Child>();
			if (c != null && c.parent != null && player != null)
				return c.parent.CanPickupEntity(c, player);
			return null;
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
