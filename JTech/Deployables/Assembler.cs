using System.Collections.Generic;
using UnityEngine;
using Oxide.Plugins.JTechCore;
using System;

namespace Oxide.Plugins.JTechDeployables {
	
	[JInfo(typeof(JTech), "Assembler", "https://i.imgur.com/R9mD3VQ.png", "This high-tech machine can assemble any item from it's blueprint with the item's ingredients and some low grade fuel.")]
	[JRequirement("vending.machine"), JRequirement("gears", 5), JRequirement("metal.refined", 20)]
	[JUpdate(10, 5)]

	public class Assembler : JDeployable {

		VendingMachine vendingMachine;

		public static void OnStartPlacing(UserInfo userInfo) {
			userInfo.ShowMessage("Placing Assembler");
		}

		public static Item GetPlaceholderItem(UserInfo userInfo) {
			// vending machine with assembler skin
			return ItemManager.CreateByName("vending.machine", 1, 1224412227);
		}

		public static void OnDeployPlaceholder(UserInfo userInfo, BaseEntity baseEntity) {
			// save the deployed placeholder
			userInfo.placingSelected = new List<BaseEntity> { baseEntity };
			userInfo.DonePlacing();
		}

		public override bool Place(UserInfo userInfo) {

			if (userInfo.placingSelected == null || userInfo.placingSelected.Count != 1)
				return false;

			data = new SaveData();
			data.SetUser(userInfo);
			
			BaseEntity placeholder = userInfo.placingSelected[0];

			data.SetTransform(placeholder.transform);
			
			if (!Spawn())
				return false;

			Effect.server.Run("assets/bundled/prefabs/fx/building/stone_gib.prefab", GetEntities()[0], 0U, Vector3.zero, Vector3.zero);

			return true;
		}

		public override bool Spawn() {

			// spawn new vending machine
			BaseEntity ent = GameManager.server.CreateEntity("assets/prefabs/deployable/vendingmachine/vendingmachine.deployed.prefab", data.GetPosition(), data.GetRotation());

			vendingMachine = ent.GetComponent<VendingMachine>();
			if (vendingMachine == null)
				return false;

			vendingMachine.skinID = 1224412227;
			vendingMachine.Spawn();
			vendingMachine.shopName = "Assembler";
			vendingMachine.SetFlag(BaseEntity.Flags.Reserved4, false, false);
			vendingMachine.UpdateMapMarker();
			vendingMachine.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);

			SetMainParent((BaseCombatEntity) ent);

			
			//DroppedItemContainer container = (DroppedItemContainer) GameManager.server.CreateEntity("assets/prefabs/misc/item drop/item_drop.prefab", Vector3.zero);
			//if (container == null)
			//	return false;

			//container.playerName = "name test";
			//container.enableSaving = false;
			//container.Spawn();

			//container.TakeFrom(new ItemContainer());

			//AddChildEntity((BaseCombatEntity) container);


			return true;
		}

		#region Hooks

		public override bool? CanAdministerVending(VendingMachine machine, BasePlayer player) {
			ShowMenu(player);
			return null;
		}

		public override bool? CanUseVending(VendingMachine machine, BasePlayer player) {
			ShowMenu(player);
			return false;
		}

		public override bool? CanVendingAcceptItem(VendingMachine machine, Item item) {
			// TODO
			// accept items into input storage
			return false;
		}

		public override object OnRotateVendingMachine(VendingMachine machine, BasePlayer player) {
			machine.transform.rotation = Quaternion.LookRotation(-machine.transform.forward, machine.transform.up);
			data.SetTransform(machine.transform);
			machine.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
			return false;
		}

		public override void OnToggleVendingBroadcast(VendingMachine machine, BasePlayer player) {
			// disable broadcast
			vendingMachine.SetFlag(BaseEntity.Flags.Reserved4, false, false);
			vendingMachine.UpdateMapMarker();
			ShowMenu(player);
		}
		
		#endregion


		public override List<Cui.ButtonInfo> GetMenuButtons(UserInfo userInfo) {
			return new List<Cui.ButtonInfo>() {
			};
		}

		public override void MenuButtonCallback(UserInfo player, string value) {

		}
	}
}