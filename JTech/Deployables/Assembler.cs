using System.Collections.Generic;
using UnityEngine;
using Oxide.Plugins.JTechCore;
using System;

namespace Oxide.Plugins.JTechDeployables {

	[JInfo(typeof(JTech), "Assembler", "https://i.imgur.com/R9mD3VQ.png")]
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

			return Spawn();
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

			SetMainParent((BaseCombatEntity) ent);

			return true;
		}
	}
}