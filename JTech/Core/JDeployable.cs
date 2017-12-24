using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins.JCore {

	public class JDeployable {
		
		// TODO
		// handle entity hooks (health, damage, repair)
		// owner, parent
		// cui menu
		// spawn/destroy 
		// load/save

		public class SaveData {
			public ulong ownerId;
			public string ownerName;
			public bool isEnabled = true;
			public float health;
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

			baseCombatEntity.gameObject.AddComponent<Child>().parent = this;

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
			MainParent.health = newhealth;
			foreach (BaseCombatEntity e in ChildEntities) {
				e.health = newhealth;
				e.SendNetworkUpdate(BasePlayer.NetworkQueue.UpdateDistance);
			}
		}

		public void Kill(BaseNetworkable.DestroyMode mode = BaseNetworkable.DestroyMode.None, bool remove = true) {
			if (!MainParent.IsDestroyed)
				MainParent.Kill(mode);
			if (remove) JDeployableManager.RemoveJDeployable(this.Id);
		}

		public class Child : MonoBehaviour {
			public JDeployable parent;
		}

		public void OnChildKilled() {
			this.Kill(BaseNetworkable.DestroyMode.Gib);
		}

		#region Child Entity Hooks

		/// <summary>
		/// OnStructureRepair hook for child entities
		/// </summary>
		public virtual void OnStructureRepair(BaseCombatEntity entity, BasePlayer player) {
			SetHealth(entity.health);
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
		public virtual bool Spawn() {
			// spawn from data
			return false;
		}


		public virtual void Update(int TickDelta) {

		}


		

	}

}