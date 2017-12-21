using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Libraries.Covalence;
using Oxide.Plugins.JCore;

namespace Oxide.Plugins.JTechDeployables {

	[JInfo(typeof(JTech), "Transport Pipe", "https://vignette.wikia.nocookie.net/play-rust/images/4/4a/Metal_Pipe_icon.png/revision/latest/scale-to-width-down/200")]
	[JRequirement("wood", 20, "segment")]

	public class TransportPipe : JDeployable {

		public enum Mode {
			SingleStack, // one stack per item
			MultiStack,  // multiple stacks per item
			SingleItem   // only one of each item
		}
		
		public StorageContainer sourcecont;
		public StorageContainer destcont;
		public string sourceContainerIconUrl;
		public string endContainerIconUrl;

		public uint sourcechild = 0;
		public uint destchild = 0;

		public Vector3 startPosition;
		public Vector3 endPosition;
		private float distance;

		public bool isWaterPipe;
		public BuildingGrade.Enum grade;
		public bool autostarter;
		public Mode mode;
		
		private static float pipesegdist = 3;
		private static Vector3 pipefightoffset = new Vector3(0.0001f, 0, 0.0001f); // every other pipe segment is offset by this to remove z fighting


		public static bool CanStartPlacing(UserInfo userInfo) {
			return true;
		}

		public static void OnStartPlacing(UserInfo userInfo) {
			userInfo.placingSelected = new List<BaseEntity>() { null, null };

			userInfo.ShowMessage("Select first container");
		}
 
		public static void OnPlacingHammerHit(UserInfo userInfo, HitInfo hit) {

			StorageContainer cont = hit.HitEntity.GetComponent<StorageContainer>();

			if (cont != null) { // we hit a StorageContainer
				
				if (checkContPrivilege(cont, userInfo.player)) { // permission for this container

					if (userInfo.placingSelected[0] == null) { // if this is the first we hit
						userInfo.placingSelected[0] = hit.HitEntity;

						userInfo.ShowMessage("Select second container");

					} else if (userInfo.placingSelected[1] == null) { // if this is the second we hit

						if (userInfo.placingSelected[0] != hit.HitEntity) { // if it's not the same as the first one

							if (userInfo.placingSelected[0] is LiquidContainer == hit.HitEntity is LiquidContainer) { // if they are the same type of container

								userInfo.placingSelected[1] = hit.HitEntity;

								userInfo.ShowMessage("Selection finished");
								userInfo.DonePlacing();

							} else {
								userInfo.ShowErrorMessage("same container error");
								userInfo.ShowMessage("Select second container", "", -1, 2f);
							}
						} else {
							userInfo.ShowErrorMessage("same container error");
							userInfo.ShowMessage("Select second container", "", -1, 2f);
						}
					}
				} else {
					// TODO no privilege error message
				}
				
			}
		}

		public override bool Place(UserInfo userInfo) {

			sourcecont = userInfo.placingSelected[0].GetComponent<StorageContainer>();
			destcont = userInfo.placingSelected[1].GetComponent<StorageContainer>();

			grade = BuildingGrade.Enum.Twigs;
			autostarter = false;
			mode = Mode.MultiStack;

			return Spawn();
		}

		public override bool Spawn() {

			//sourceContainerIconUrl;
			//endContainerIconUrl;

			isWaterPipe = sourcecont is LiquidContainer;

			startPosition = sourcecont.CenterPoint() + containeroffset(sourcecont);
			endPosition = destcont.CenterPoint() + containeroffset(destcont);

			distance = Vector3.Distance(startPosition, endPosition);
			Quaternion rotation = Quaternion.LookRotation(endPosition - startPosition) * Quaternion.Euler(90, 0, 0);

			//isStartable();

			// TODO spawn pillars

			int segments = (int) Mathf.Ceil(distance / pipesegdist);
			float segspace = (distance - pipesegdist) / (segments - 1);

			for (int i = 0; i < segments; i++) {
				
				// create pillar

				BaseEntity ent;

				if (i == 0) {
					// the position thing centers the pipe if there is only one segment
					ent = GameManager.server.CreateEntity("assets/prefabs/building core/pillar/pillar.prefab", (segments == 1) ? (startPosition + ((rotation * Vector3.up) * ((distance - pipesegdist) * 0.5f))) : startPosition, rotation);
					SetMainParent(ent);
				} else {
					ent = GameManager.server.CreateEntity("assets/prefabs/building core/pillar/pillar.prefab", Vector3.up * (segspace * i) + ((i % 2 == 0) ? Vector3.zero : pipefightoffset));

				}

				ent.enableSaving = false;

				BuildingBlock block = ent.GetComponent<BuildingBlock>();

				if (block != null) {
					block.grounded = true;
					block.grade = grade;
					block.enableSaving = false;
					block.Spawn();
					block.SetHealthToMax();
				}
				

				// xmas lights

				//BaseEntity lights = GameManager.server.CreateEntity("assets/prefabs/misc/xmas/christmas_lights/xmas.lightstring.deployed.prefab", (Vector3.up * pipesegdist * 0.5f) + (Vector3.forward * 0.13f) + (Vector3.up * (segspace * i) + ((i % 2 == 0) ? Vector3.zero : pipefightoffset)), Quaternion.Euler(0, -60, 90));
				//lights.enableSaving = false;
				//lights.Spawn();
				//lights.SetParent(mainparent);
				//jPipeSegChildLights.Attach(lights, this);

				if (i != 0)
					AddChildEntity(ent);

			}

			return false;
		}



		private static bool checkContPrivilege(StorageContainer cont, BasePlayer p) => cont.CanOpenLootPanel(p) && checkBuildingPrivilege(p);

		private static bool checkBuildingPrivilege(BasePlayer p) {
			//if (permission.UserHasPermission(p.UserIDString, "jpipes.admin"))
			//	return true;
			return p.CanBuild();
		}

		//private static bool checkPipeOverlap(StorageContainer start, jPipeData data) {
		//	uint s = getcontfromid(data.s, data.cs).net.ID;
		//	uint e = getcontfromid(data.d, data.cd).net.ID;

		//	foreach (var p in rps)
		//		if ((p.Value.sourcecont.net.ID == s && p.Value.destcont.net.ID == e) || (p.Value.sourcecont.net.ID == e && p.Value.destcont.net.ID == s))
		//			return true;
		//	return false;
		//}

		private static Vector3 containeroffset(BaseEntity e) {
			if (e is BoxStorage)
				return Vector3.zero;
			else if (e is BaseOven) {
				string panel = e.GetComponent<BaseOven>().panelName;

				if (panel == "largefurnace")
					return Vector3.up * -1.5f;
				else if (panel == "smallrefinery")
					return e.transform.rotation * new Vector3(-1, 0, -0.1f);
				else if (panel == "bbq")
					return Vector3.up * 0.03f;
				else
					return Vector3.up * -0.3f;
				//} else if (e is ResourceExtractorFuelStorage) {
				//if (e.GetComponent<StorageContainer>().panelName == "fuelstorage") {
				//    return contoffset.pumpfuel;
				//} else {
				//    return e.transform.rotation * contoffset.pumpoutput;
				//}
			} else if (e is AutoTurret) {
				return Vector3.up * -0.58f;
			} else if (e is SearchLight) {
				return Vector3.up * -0.5f;
			} else if (e is WaterCatcher) {
				return Vector3.up * -0.6f;
			} else if (e is LiquidContainer) {
				if (e.GetComponent<LiquidContainer>()._collider.ToString().Contains("purifier"))
					return Vector3.up * 0.25f;
				return Vector3.up * 0.2f;
			}
			return Vector3.zero;
		}
		private static bool isStartable(BaseEntity e, int destchildid) => e is BaseOven || e is Recycler || destchildid == 2;
		
	}
}