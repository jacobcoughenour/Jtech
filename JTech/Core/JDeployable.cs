using Newtonsoft.Json;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins.JTechCore {

	public class JDeployable {

		// TODO cui menu

		public class SaveData {

			public ulong ownerId;
			public string ownerName;
			public bool isEnabled = true;
			public float health;
			public List<float> transform;
			public Dictionary<string, string> custom = new Dictionary<string, string>();

			/// <summary>
			/// Set userInfo as the owner
			/// </summary>
			/// <param name="userInfo"></param>
			public void SetUser(UserInfo userInfo) {
				this.ownerId = userInfo.player.userID;
				this.ownerName = userInfo.player.displayName;
			}

			/// <summary>
			/// Set transform
			/// </summary>
			/// <param name="transform"></param>
			public void SetTransform(Transform transform) {
				this.transform = new List<float>() {
					transform.position.x,
					transform.position.y,
					transform.position.z,

					transform.rotation.x,
					transform.rotation.y,
					transform.rotation.z,
					transform.rotation.w,
				};
			}

			/// <summary>
			/// Get Position
			/// </summary>
			public Vector3 GetPosition() {
				if (this.transform == null || this.transform.Count != 7)
					return Vector3.zero;
				return new Vector3(transform[0], transform[1], transform[2]);
			}

			/// <summary>
			/// Get Rotation
			/// </summary>
			public Quaternion GetRotation() {
				if (this.transform == null || this.transform.Count != 7)
					return Quaternion.identity;
				return new Quaternion(transform[3], transform[4], transform[5], transform[6]);
			}

			/// <summary>
			/// Set custom data value
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="name"></param>
			/// <param name="value"></param>
			public void Set(string name, string value) {
				if (custom.ContainsKey(name))
					custom[name] = value;
				else
					custom.Add(name, value);
			}

			/// <summary>
			/// Set custom data value
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="name"></param>
			/// <param name="value"></param>
			public void Set(string name, object value) {
				if (custom.ContainsKey(name))
					custom[name] = value.ToString();
				else
					custom.Add(name, value.ToString());
			}

			/// <summary>
			/// Get custom data value
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="name"></param>
			/// <param name="defaultvalue"></param>
			/// <returns></returns>
			public string Get(string name, string defaultvalue = "") {
				string value;
				if (this.custom.TryGetValue(name, out value))
					return value;
				return defaultvalue;
			}

			public bool Has(params string[] names) {
				foreach (string s in names) {
					if (!custom.ContainsKey(s))
						return false;
				}
				return true;
			}
		}

		public int Id;
		public SaveData data;
		private bool isBeingDestroyed = false;

		private BaseCombatEntity MainParent;
		private List<BaseCombatEntity> ChildEntities = new List<BaseCombatEntity>();

		public void SetMainParent(BaseCombatEntity baseCombatEntity) {
			baseCombatEntity.gameObject.AddComponent<Child>().parent = this;
			
			MainParent = baseCombatEntity;
			MainParent.enableSaving = false;
		}

		public List<BaseCombatEntity> GetEntities() {
			var ents = new List<BaseCombatEntity>();
			ents.Add(MainParent);
			ents.AddRange(ChildEntities);
			return ents;
		}

		/// <summary>
		/// Parent entity to main parent.
		/// Note: make sure you .Spawn() the entity first.
		/// </summary>
		/// <param name="baseEntity"></param>
		public void AddChildEntity(BaseCombatEntity baseCombatEntity) {
			if (MainParent == null)
				return;

			Child c = baseCombatEntity.gameObject.GetComponent<Child>();
			if (c != null)
				c.parent = this;
			else {
				baseCombatEntity.gameObject.AddComponent<Child>().parent = this;
			}

			baseCombatEntity.SetParent(MainParent);
			baseCombatEntity.enableSaving = false;
			ChildEntities.Add(baseCombatEntity);
		}

		/// <summary>
		/// Use this to change the health of your deployable.
		/// This will set the health of all child entities.
		/// </summary>
		/// <param name="newhealth"></param>
		public void SetHealth(float newhealth) {
			data.health = newhealth;
			if (MainParent != null)
				MainParent.health = newhealth;
			foreach (BaseCombatEntity e in ChildEntities) {
				e.health = newhealth;
				e.SendNetworkUpdate(BasePlayer.NetworkQueue.UpdateDistance);
			}
		}

		public void Kill(BaseNetworkable.DestroyMode mode = BaseNetworkable.DestroyMode.None, bool remove = true) {
			if (isBeingDestroyed)
				return;

			if (MainParent != null && !MainParent.IsDestroyed) {
				isBeingDestroyed = true;
				MainParent.Kill(mode);
			}

			if (remove)
				JDeployableManager.RemoveJDeployable(this.Id);
		}

		public class Child : MonoBehaviour {
			public JDeployable parent;
		}

		#region Child Entity Hooks

		/// <summary>
		/// OnHammerHit hook for child entities
		/// </summary>
		public virtual void OnHammerHit(BasePlayer player, HitInfo hit) {
		}

		/// <summary>
		/// OnStructureRepair hook for child entities
		/// </summary>
		public virtual void OnStructureRepair(BaseCombatEntity entity, BasePlayer player) {
			SetHealth(entity.health);
		}

		/// <summary>
		/// OnStructureRotate hook for child entities
		/// </summary>
		public virtual void OnStructureRotate(BaseCombatEntity entity, BasePlayer player) {
		}

		/// <summary>
		/// OnStructureUpgrade hook for child entities
		/// </summary>
		public virtual bool? OnStructureUpgrade(Child child, BasePlayer player, BuildingGrade.Enum grade) {
			return null;
		}

		/// <summary>
		/// CanPickupEntity hook for child entities
		/// </summary>
		public virtual bool? CanPickupEntity(Child child, BasePlayer player) {
			return null;
		}

		public virtual bool? CanAdministerVending(VendingMachine machine, BasePlayer player) {
			return null;
		}

		public virtual bool? CanUseVending(VendingMachine machine, BasePlayer player) {
			return null;
		}

		public virtual bool? CanVendingAcceptItem(VendingMachine machine, Item item) {
			return null;
		}

		public virtual object OnRotateVendingMachine(VendingMachine machine, BasePlayer player) {
			return null;
		}

		public virtual void OnToggleVendingBroadcast(VendingMachine machine, BasePlayer player) {
		}

		#endregion

		#region Placing

		/// <summary>
		/// Can User start placing this deployable?
		/// Item requirements are already handled by UserInfo.
		/// This is if you want to add custom requirements to creating your deployable.
		/// </summary>
		/// <returns>if user can start placing this deployable</returns>
		public static bool CanStartPlacing(UserInfo userInfo) {
			return true;
		}

		/// <summary>
		/// Puts a placeholder item in player's hotbar used for placing this deployable.
		/// OnDeployPlaceholder() is called when placeholder item is deployed.
		/// </summary>
		/// <param name="userInfo"></param>
		/// <returns></returns>
		public static Item GetPlaceholderItem(UserInfo userInfo) {
			return null;
		}

		/// <summary>
		/// Called when player deploys the placeholder item.
		/// Use this to add to the deployed entity or destroy it and replace it.
		/// Make sure you either call userInfo.DonePlacing() or userInfo.CancelPlacing().
		/// </summary>
		/// <param name="userInfo"></param>
		/// <param name="baseEntity"></param>
		public static void OnDeployPlaceholder(UserInfo userInfo, BaseNetworkable baseNetworkable) {
		}

		/// <summary>
		/// Called when player starts placing this deployable.
		/// Use this for displaying instructions via userInfo.ShowMessage().
		/// To stop placing you can either call userInfo.DonePlacing() or userInfo.CancelPlacing().
		/// </summary>
		public static void OnStartPlacing(UserInfo userInfo) {

		}

		/// <summary>
		/// Called after userInfo.DonePlacing() or userInfo.CancelPlacing().
		/// Use this to clean up anything left over from placing your deployable (visual aids, placeholder items, etc).
		/// </summary>
		public static void OnEndPlacing(UserInfo userInfo) {

		}

		/// <summary>
		/// Called when player is placing this deployable and hits an entity with a hammer.
		/// Use this if you want the player to select existing entities.
		/// Use userInfo.placingdata to store selected entities or other data.
		/// </summary>
		public static void OnPlacingHammerHit(UserInfo userInfo, HitInfo hit) {

		}
		
		/// <summary>
		/// Spawn a deployable from placing command and set default values.
		/// </summary>
		/// <returns>If successfully placed</returns>
		public virtual bool Place(UserInfo userInfo) {
			// set deployable variables here
			return Spawn();
		}

		#endregion
		
		/// <summary>
		/// Spawn your deployable from this.data
		/// </summary>
		/// <returns></returns>
		public virtual bool Spawn(bool placing = false) {
			// spawn from data
			return false;
		}

		public long _lastUpdate;

		/// <summary>
		/// Update loop
		/// </summary>
		/// <param name="timeDelta">elapsed time between updates in seconds</param>
		/// <returns></returns>
		public virtual bool Update(float timeDelta) {
			return true;
		}

		#region CUI

		private HashSet<UserInfo> _playerslookingatmenu = new HashSet<UserInfo>();

		public void ShowMenu(BasePlayer player) => ShowMenu(UserInfo.Get(player));

		public void ShowMenu(UserInfo userInfo) {
			userInfo.ShowMenu(this);
			_playerslookingatmenu.Add(userInfo);
		}

		public void HideMenu(BasePlayer player) => HideMenu(UserInfo.Get(player));

		public void HideMenu(UserInfo userInfo) {
			userInfo.HideMenu();
			_playerslookingatmenu.Remove(userInfo);
		}

		public void HideMenuAll() {
			HashSet<UserInfo> p = _playerslookingatmenu;
			foreach (var ui in p)
				HideMenu(ui);
		}

		public void UpdateMenu() {
			HashSet<UserInfo> p = _playerslookingatmenu;
			foreach (var ui in p) {
				ui.HideMenu();
				ui.ShowMenu(this);
			}
		}

		public virtual Dictionary<string, string> GetMenuInfo(UserInfo userInfo) {
			return new Dictionary<string, string>() {
				{ "Owner", data.ownerName },
				{ "Health", data.health.ToString() }
			};
		}

		public virtual void GetMenuContent(CuiElementContainer elements, string parent, UserInfo userInfo) {

			string main = elements.Add(
				new CuiPanel {
					Image = { Color = "0.251 0.769 1 0.25" },
					RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
				}, parent
			);

			float topheight = 0.075f;

			// top drop shadow
			elements.Add(
				new CuiPanel {
					Image = { Color = "0.004 0.341 0.608 0.15" },
					RectTransform = { AnchorMin = $"0 {1 - topheight - 0.008f}", AnchorMax = $"1 1" },
				}, main
			);

			elements.Add(
				new CuiPanel {
					Image = { Color = "0.004 0.341 0.608 0.15" },
					RectTransform = { AnchorMin = $"0 {1 - topheight - 0.016f}", AnchorMax = $"1 1" },
				}, main
			);
			
			// top area
			string top = elements.Add(
				new CuiPanel {
					Image = { Color = "0.251 0.769 1 0.2" },
					RectTransform = { AnchorMin = $"0 {1 - topheight}", AnchorMax = $"0.996 0.996" },
				}, main
			);

			elements.Add(
				new CuiLabel {
					Text = { Text = $"INFORMATION", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "1 1 1 0.75" },
					RectTransform = { AnchorMin = "0.03 0", AnchorMax = "1 1" }
				}, top
			);


			// Deployable Info

			float separator = 0.25f;
			float gap = 0.015f;
			float lineheight = 0.07f;

			// info area
			string infoarea = elements.Add(
				new CuiPanel {
					Image = { Color = "0 0 0 0" },
					RectTransform = { AnchorMin = "0 0", AnchorMax = $"1 {1 - topheight}" },
				}, main
			);

			Dictionary<string, string> info = GetMenuInfo(userInfo);

			for (int i = 0; i < info.Count; i++) {

				elements.Add(
					new CuiLabel {
						Text = { Text = info.Keys.ElementAt(i), FontSize = 12, Align = TextAnchor.MiddleRight, Color = "1 1 1 0.5" },
						RectTransform = { AnchorMin = $"0 {1 - gap - gap - (lineheight * i) - lineheight}", AnchorMax = $"{separator - gap} {1 - gap - gap - (lineheight * i)}" }
					}, infoarea
				);
				elements.Add(
					new CuiLabel {
						Text = { Text = info.Values.ElementAt(i), FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "1 1 1 0.9" },
						RectTransform = { AnchorMin = $"{separator + gap} {1 - gap - gap - (lineheight * i) - lineheight}", AnchorMax = $"1 {1 - gap - gap - (lineheight * i)}" }
					}, infoarea
				);
			}

		}


		public virtual List<Cui.ButtonInfo> GetMenuButtons(UserInfo userInfo) {
			return new List<Cui.ButtonInfo>();
		}

		public void MenuOnOffButton(UserInfo player) {
			data.isEnabled = !data.isEnabled;
			UpdateMenu();
		}

		public virtual void MenuButtonCallback(UserInfo player, string value) {

		}


		#endregion

	}
}