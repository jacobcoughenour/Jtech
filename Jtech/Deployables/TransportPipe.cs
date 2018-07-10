using System.Collections.Generic;
using UnityEngine;
using Oxide.Plugins.JtechCore;
using Oxide.Plugins.JtechCore.Util;
using System;
using System.Linq;
using Oxide.Game.Rust.Cui;

namespace Oxide.Plugins.JtechDeployables {

	[JInfo(typeof(Jtech), "Transport Pipe", "https://vignette.wikia.nocookie.net/play-rust/images/4/4a/Metal_Pipe_icon.png/revision/latest/scale-to-width-down/200", "Transfers items or liquids between containers.  Upgrade it with the hammer to increase the flow rate and functionality.")]
	[JRequirement("wood", 20, "segment")]
	[JUpdate(4, 16)]

	public class TransportPipe : JDeployable {
		
		// TODO Item Filter (add container management to JDeployable)
		// TODO Fueling Mode
		// TODO Furnace Splitter and cui

		public static List<int> flowrates = new List<int>() { 1, 5, 10, 30, 50 };
		public static List<int> itemfiltersizes = new List<int>() { 0, 6, 12, 18, 30 };

		public static string[] upgradeeffect = new string[] {
			"assets/bundled/prefabs/fx/build/promote_wood.prefab",
			"assets/bundled/prefabs/fx/build/promote_wood.prefab",
			"assets/bundled/prefabs/fx/build/promote_stone.prefab",
			"assets/bundled/prefabs/fx/build/promote_metal.prefab",
			"assets/bundled/prefabs/fx/build/promote_toptier.prefab",
		};

		public enum Mode {
			MultiStack,  // multiple stacks per item
			SingleStack, // one stack per item
			SingleItem,  // only one of each item
			Fueling,
			Count = 4
		}

		public static string[] ModeNames = new string[] {
			"Multi Stack",
			"Single Stack",
			"Single Item",
			"Fueling"
		};


		// these are just cached values that will not be saved

		public StorageContainer sourcecont;
		public StorageContainer destcont;
		private uint sourcechildid;
		private uint destchildid;
		public string sourceContainerIconUrl;
		public string destContainerIconUrl;
		public Vector3 startPosition;
		public Vector3 endPosition;
		private float distance;
		public bool isWaterPipe;
		private bool destisstartable;
		private int flowrate;
		private Mode mode;

		private static float pipesegdist = 3;
		private static Vector3 pipefightoffset = new Vector3(0.001f, 0.001f, 0); // every other pipe segment is offset by this to remove z fighting


		public new static bool CanStartPlacing(UserInfo userInfo) {
			return true;
		}

		public new static void OnStartPlacing(UserInfo userInfo) {
			userInfo.placingSelected = new List<BaseEntity>() { null, null };

			userInfo.ShowMessage("Select first container");
		}
 
		public new static void OnPlacingHammerHit(UserInfo userInfo, HitInfo hit) {

			StorageContainer cont = hit.HitEntity.GetComponent<StorageContainer>();

			if (cont != null) { // we hit a StorageContainer
				
				if (CheckContPrivilege(cont, userInfo.player)) { // permission for this container

					if (userInfo.placingSelected[0] == null) { // if this is the first we hit
						userInfo.placingSelected[0] = hit.HitEntity;

						userInfo.ShowMessage("Select second container");

					} else if (userInfo.placingSelected[1] == null) { // if this is the second we hit
						if (userInfo.placingSelected[0] != hit.HitEntity) { // if it's not the same as the first one
							if (userInfo.placingSelected[0] is LiquidContainer == hit.HitEntity is LiquidContainer) { // if they are the same type of container
								if (!isPipeOverlapping(userInfo.placingSelected[0], hit.HitEntity)) { // if not overlapping

									userInfo.placingSelected[1] = hit.HitEntity;
									userInfo.DonePlacing();

								} else {
									userInfo.ShowErrorMessage("overlap error");
									userInfo.ShowMessage("Select second container", "", -1, 2f);
								}
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
			
			data = new SaveData();
			data.SetUser(userInfo);

			uint scid;
			uint dcid;
			data.Set("sourceid", GetIdFromContainer(userInfo.placingSelected[0], out scid));
			data.Set("destid", GetIdFromContainer(userInfo.placingSelected[1], out dcid));
			data.Set("sourcechildid", scid);
			data.Set("destchildid", dcid);

			data.Set("grade", "0");
			data.Set("mode", "0");

			return Spawn(true);
		}

		public override void OnHammerHit(BasePlayer player, HitInfo hit) {
			ShowMenu(player);
		}

		public override bool? OnStructureUpgrade(Child child, BasePlayer player, BuildingGrade.Enum grade) {

			var ents = GetEntities();
			for (int i = 0; i < ents.Count; i++) {

				BuildingBlock b = ents[i].gameObject.GetComponent<BuildingBlock>();
				ents[i].gameObject.GetComponent<Child>()?.RunDelayed(i * 0.2f, () => {
					if (b == null)
						return;
					b.SetGrade(grade);
					b.SetHealthToMax();
					b.SendNetworkUpdate(BasePlayer.NetworkQueue.UpdateDistance);
					Effect.server.Run(upgradeeffect[(int) grade], b.transform.position + (b.transform.up * 1.5f), Vector3.up);
				});
			}

			data.Set("grade", ((int) grade).ToString());
			flowrate = flowrates[(int) grade];

			return null;
		}

		public override bool Spawn(bool placing = false) {
			
			if (!(data.Has("sourceid", "destid", "sourcechildid", "destchildid")))
				return false;

			uint sourceid = uint.Parse(data.Get("sourceid"));
			uint destid = uint.Parse(data.Get("destid"));
			sourcechildid = uint.Parse(data.Get("sourcechildid"));
			destchildid = uint.Parse(data.Get("destchildid"));
			
			sourcecont = GetChildContainer(BaseNetworkable.serverEntities.Find(sourceid), sourcechildid);
			destcont = GetChildContainer(BaseNetworkable.serverEntities.Find(destid), destchildid);

			if (sourcecont == null || destcont == null || sourcecont == destcont)
				return false;

			sourceContainerIconUrl = Icons.GetContainerIconURL(sourcecont, 100);
			destContainerIconUrl = Icons.GetContainerIconURL(destcont, 100);

			isWaterPipe = sourcecont is LiquidContainer;
			destisstartable = IsStartable(destcont);
			flowrate = flowrates[int.Parse(data.Get("grade", "0"))];
			mode = (Mode) int.Parse(data.Get("mode", "0"));

			startPosition = sourcecont.CenterPoint() + ContainerOffset(sourcecont);
			endPosition = destcont.CenterPoint() + ContainerOffset(destcont);

			distance = Vector3.Distance(startPosition, endPosition);
			Quaternion rotation = Quaternion.LookRotation(endPosition - startPosition);

			//isStartable();

			// spawn pillars

			int segments = (int) Mathf.Ceil(distance / pipesegdist);
			float segspace = (distance - pipesegdist) / (segments - 1);
			startPosition += ((rotation * Vector3.forward) * pipesegdist * 0.5f) + ((rotation * Vector3.down) * 0.7f);

			for (int i = 0; i < segments; i++) {

				// create pillar

				BaseEntity ent;

				if (i == 0) {
					// the position thing centers the pipe if there is only one segment
					ent = GameManager.server.CreateEntity("assets/prefabs/building core/wall.low/wall.low.prefab", (segments == 1) ? (startPosition + ((rotation * Vector3.up) * ((distance - pipesegdist) * 0.5f))) : startPosition, rotation);
					data.SetTransform(ent.transform);
					SetMainParent((BaseCombatEntity) ent);
				} else {
					ent = GameManager.server.CreateEntity("assets/prefabs/building core/wall.low/wall.low.prefab", Vector3.forward * (segspace * i) + ((i % 2 == 0) ? Vector3.zero : pipefightoffset));
					//ent = GameManager.server.CreateEntity("assets/prefabs/building core/pillar/pillar.prefab", startPosition);
				}

				ent.enableSaving = false;

				BuildingBlock block = ent.GetComponent<BuildingBlock>();

				if (block != null) {
					block.grounded = true;
					block.grade = (BuildingGrade.Enum) int.Parse(data.Get("grade", "0"));
					block.enableSaving = false;
					block.Spawn();
					block.SetHealthToMax();
				} else
					return false;
				
				
				//((DecayEntity) ent).GetNearbyBuildingBlock();
				
				if (i != 0) {

					if (placing) { // placing animation
						ent.gameObject.AddComponent<Child>()?.RunDelayed(i * 0.2f, () => {
							AddChildEntity((BaseCombatEntity) ent);
							Effect.server.Run("assets/bundled/prefabs/fx/build/promote_wood.prefab", ent.transform.position + (ent.transform.up * (segspace * 0.5f)), Vector3.up);
						});
					} else {
						AddChildEntity((BaseCombatEntity) ent);
					}
				}
				
				// xmas lights

				//BaseEntity lights = GameManager.server.CreateEntity("assets/prefabs/misc/xmas/christmas_lights/xmas.lightstring.deployed.prefab", (Vector3.up * pipesegdist * 0.5f) + (Vector3.forward * 0.13f) + (Vector3.up * (segspace * i) + ((i % 2 == 0) ? Vector3.zero : pipefightoffset)), Quaternion.Euler(0, -60, 90));
				//lights.Spawn();
				//AddChildEntity((BaseCombatEntity) lights);
			}

			if (placing) {
				SetHealth(GetEntities()[0].MaxHealth());
			} else
				SetHealth(data.health);

			return true;
		}

		public override bool Update(float timeDelta) {
			
			// if container is destroyed, kill pipe
			if (sourcecont == null || destcont == null) {
				Kill(BaseNetworkable.DestroyMode.Gib);
				return false;
			}
			
			if (data.isEnabled) {
				
				if (sourcecont.inventory.itemList.Count > 0 && sourcecont.inventory.itemList[0] != null) {

					int amounttotake = Mathf.FloorToInt(timeDelta * flowrate);

					if (amounttotake < 1)
						return false;

					if (isWaterPipe) { // water pipe

						Item itemtomove = sourcecont.inventory.itemList[0];

						if (destcont.inventory.itemList.Count == 1) {
							Item slot = destcont.inventory.itemList[0];

							if (slot.CanStack(itemtomove)) {

								int maxstack = slot.MaxStackable();
								if (slot.amount < maxstack) {
									if ((maxstack - slot.amount) < amounttotake)
										amounttotake = maxstack - slot.amount;
									MoveItem(itemtomove, amounttotake, destcont, -1);
								}
							}
						} else {
							MoveItem(itemtomove, amounttotake, destcont, -1);
						}

					} else { // item pipe

						Item itemtomove = FindItem();

						// move the item
						if (itemtomove != null && CanAcceptItem(itemtomove)) {

							if (mode == Mode.SingleStack) {

								Item slot = destcont.inventory.FindItemsByItemID(itemtomove.info.itemid).OrderBy<Item, int>((Func<Item, int>) (x => x.amount)).FirstOrDefault<Item>();

								if (slot != null) { // if there is already a stack of itemtomove in destcontainer
									if (slot.CanStack(itemtomove)) { // can we stack this item?

										int maxstack = slot.MaxStackable();
										if (slot.amount < maxstack) {
											if ((maxstack - slot.amount) < amounttotake)
												amounttotake = maxstack - slot.amount; // amount to add to make it to max stack size
																					   //pipe.moveitem(itemtomove, amounttotake, pipe.destcont, (pipe.fsplit) ? pipe.fsstacks : -1);
											MoveItem(itemtomove, amounttotake, destcont, -1);
											TurnOnDest();
										}
									}
								} else {
									//pipe.moveitem(itemtomove, amounttotake, pipe.destcont, (pipe.fsplit) ? pipe.fsstacks : -1);
									MoveItem(itemtomove, amounttotake, destcont, -1);
									TurnOnDest();
								}
							} else if (mode == Mode.SingleItem) {
								Item slot = destcont.inventory.FindItemsByItemID(itemtomove.info.itemid).OrderBy<Item, int>((Func<Item, int>) (x => x.amount)).FirstOrDefault<Item>();

								if (slot == null) {
									//pipe.moveitem(itemtomove, amounttotake, pipe.destcont, (pipe.fsplit) ? pipe.fsstacks : -1);
									MoveItem(itemtomove, 1, destcont, -1);
									TurnOnDest();
								}
							} else if (mode == Mode.Fueling) {
								Item slot = destcont.inventory.FindItemsByItemID(itemtomove.info.itemid).OrderBy<Item, int>((Func<Item, int>) (x => x.amount)).FirstOrDefault<Item>();

								if (slot == null) {
									//pipe.moveitem(itemtomove, amounttotake, pipe.destcont, (pipe.fsplit) ? pipe.fsstacks : -1);
									MoveItem(itemtomove, 1, destcont, -1);
									TurnOnDest();
								}
							} else if (mode == Mode.MultiStack) {
								MoveItem(itemtomove, amounttotake, destcont, -1);
								TurnOnDest();
							}
						}

					}
				}


				
			}

			return true;
		}

		private static void MoveItem(Item itemtomove, int amounttotake, StorageContainer cont, int stacks) {

			if (itemtomove.amount > amounttotake)
				itemtomove = itemtomove.SplitItem(amounttotake);

			//if ((BaseEntity) cont is BaseOven && stacks > -1) {
			//	if (FurnaceSplitter != null)
			//		FurnaceSplitter?.Call("MoveSplitItem", itemtomove, (BaseEntity) cont, stacks);
			//	else
			//		itemtomove.MoveToContainer(cont.inventory);
			//} else {
			itemtomove.MoveToContainer(cont.inventory);
			//}
		}

		// TODO this should probably be renamed or moved inline
		private bool CanAcceptItem(Item itemtomove) {
			return destcont.inventory.CanAcceptItem(itemtomove, -1) == ItemContainer.CanAcceptResult.CanAccept && destcont.inventory.CanTake(itemtomove);
		}

		private Item FindItem() {

			foreach (Item i in sourcecont.inventory.itemList) { // for each item in source container
				//if (filteritems.Count == 0 || filteritems.Contains(i.info.itemid)) { // if filter is empty or contains this item
					if (!(sourcecont is Recycler) || (sourcecont is Recycler && i.position > 5)) { // if source is recycler then only take items from the output

						if (destcont is BaseOven) { // only send Burnable or Cookable to BaseOven
							if ((i.info.GetComponent<ItemModBurnable>()) || (i.info.GetComponent<ItemModCookable>()))
								return i;
						} else if (destcont is Recycler) { // only send recyclables to recycler
							if (i.info.Blueprint != null)
								return i;
						} else {
							return i;
						}
					}
				//}
			}
			return null;
		}

		private static bool CheckContPrivilege(StorageContainer cont, BasePlayer p) => cont.CanOpenLootPanel(p) && CheckBuildingPrivilege(p);

		private static bool CheckBuildingPrivilege(BasePlayer p) {
			//if (permission.UserHasPermission(p.UserIDString, "jpipes.admin"))
			//	return true;
			return p.CanBuild();
		}
		
		private static bool isPipeOverlapping(BaseEntity sourcecont, BaseEntity destcont) {
			
			uint s = sourcecont.net.ID;
			uint d = destcont.net.ID;

			List<JDeployable> pipes = JDeployableManager.GetSpawned<TransportPipe>();
			if (pipes.Count == 0)
				return false;

			foreach (TransportPipe p in pipes) {
				if ((p.sourcecont.net.ID == s && p.destcont.net.ID == d) || (p.sourcecont.net.ID == d && p.destcont.net.ID == s))
					return true;
			}
			return false;
		}

		// find storage container from id and child id
		private static StorageContainer GetContainerFromId(uint id, uint cid = 0) => GetChildContainer(BaseNetworkable.serverEntities.Find(id), cid);

		// find storage container from parent and child id
		private static StorageContainer GetChildContainer(BaseNetworkable parent, uint id = 0) {
			if (id != 0) {
				BaseResourceExtractor ext = parent?.GetComponent<BaseResourceExtractor>();
				if (ext != null) {
					foreach (var c in ext.children) {
						if (c is ResourceExtractorFuelStorage && c.GetComponent<ResourceExtractorFuelStorage>().panelName == ((id == 2) ? "fuelstorage" : "generic"))
							return c.GetComponent<StorageContainer>();
					}
				}
				//return parent.GetComponent<StorageContainer>();
			}
			return parent?.GetComponent<StorageContainer>();
		}

		private static uint GetIdFromContainer(BaseEntity cont, out uint cid) {

			ResourceExtractorFuelStorage stor = cont.GetComponent<ResourceExtractorFuelStorage>();

			if (stor != null) {
				switch (stor.panelName) {
					case "generic":
						cid = 1;
						break;
					case "fuelstorage":
						cid = 2;
						break;
					default:
						cid = 0;
						break;
				}

				return stor.parentEntity.uid;
			}

			cid = 0;
			return cont.net.ID;
		}

		private static Vector3 ContainerOffset(BaseEntity e) {
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
		
		/// <summary>
		/// Is the given BaseEntity startable?
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool IsStartable(BaseEntity e) => e is BaseOven || e is Recycler || destchildid == 2;
		

		private void TurnOnDest() {
			if (!bool.Parse(data.Get("autostart", "false")) || !destisstartable)
				return;

			BaseEntity e = destcont;
			if (e is BaseOven) {
				e.GetComponent<BaseOven>().StartCooking();
			} else if (e is Recycler) {
				e.GetComponent<Recycler>().StartRecycling();
			} else if (destchildid == 2) {
				BaseEntity ext = (BaseEntity) BaseNetworkable.serverEntities.Find(e.parentEntity.uid);
				if (ext != null)
					ext.GetComponent<MiningQuarry>().EngineSwitch(true);
			}
		}

		private void ChangeDirection() {
			
			uint sourceid = uint.Parse(data.Get("destid"));
			uint destid = uint.Parse(data.Get("sourceid"));
			sourcechildid = uint.Parse(data.Get("destchildid"));
			destchildid = uint.Parse(data.Get("sourcechildid"));
			
			sourcecont = GetChildContainer(BaseNetworkable.serverEntities.Find(sourceid), sourcechildid);
			destcont = GetChildContainer(BaseNetworkable.serverEntities.Find(destid), destchildid);

			if (sourcecont == null || destcont == null)
				return;

			sourceContainerIconUrl = Icons.GetContainerIconURL(sourcecont, 100);
			destContainerIconUrl = Icons.GetContainerIconURL(destcont, 100);

			uint scid;
			uint dcid;
			data.Set("sourceid", GetIdFromContainer(sourcecont, out scid));
			data.Set("destid", GetIdFromContainer(destcont, out dcid));
			data.Set("sourcechildid", scid);
			data.Set("destchildid", dcid);

			destisstartable = IsStartable(destcont);

			if (!destisstartable && mode == Mode.Fueling) {
				mode = 0;
				data.Set("mode", "0");
			}

			UpdateMenu();
		}

		private void NextMode() {

			mode = (mode == Mode.Count - 1 || // if next mode is count
				(!destisstartable && mode == Mode.Fueling - 1)) ?  // if next mode is fueling and dest is not startable
				0 : mode + 1;
			data.Set("mode", (int) mode);

			UpdateMenu();
		}

		private void ToggleAutoStarter() {
			data.Set("autostart", !bool.Parse(data.Get("autostart", "false")));

			UpdateMenu();
		}

		#region CUI
		
		public override void GetMenuContent(CuiElementContainer elements, string parent, UserInfo userInfo) {

			//TODO show if dest is running

			// main/parent is always at a 1:1 aspect ratio to make sizing easier
			string main = elements.Add(
				new CuiPanel {
					Image = { Color = "0 0 0 0" },
					RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
				}, parent
			);

			float pipey = 0.8f;
			float pipewidth = 0.275f;
			float pipeheight = 0.0275f;
			float arrowoffset = 0.005f;
			float conticonsize = 0.125f;

			// slight outline around the flowpipe
			Cui.FakeDropShadow(elements, main, 0.5f - pipewidth, pipey - pipeheight, 0.5f + pipewidth, pipey + pipeheight, 0f, 0.01f, 1, $"{Cui.Colors.Blue} 0.1");

			string flowpipe = elements.Add(
				new CuiPanel {
					Image = { Color = $"{Cui.Colors.Blue} 0.5" },
					RectTransform = { AnchorMin = $"{0.5f - pipewidth} {pipey - pipeheight}", AnchorMax = $"{0.5f + pipewidth} {pipey + pipeheight}" },
				}, main
			);

			elements.Add(
				Cui.AddOutline(
				new CuiLabel {
					Text = { Text = new String('>', int.Parse(data.Get("grade", "0")) + 1), FontSize = 20, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
					RectTransform = { AnchorMin = $"{0.5f - pipewidth} {pipey - pipeheight * 2 + arrowoffset}", AnchorMax = $"{0.5f + pipewidth} {pipey + pipeheight * 2 + arrowoffset}" }
				}, main, $"{Cui.Colors.DarkBlue} 0.8")
			);

			elements.Add(
				Cui.CreateIcon(main, $"{0.5f - pipewidth - conticonsize} {pipey - conticonsize}", $"{0.5f - pipewidth + conticonsize} {pipey + conticonsize}", sourceContainerIconUrl)
			);
			elements.Add(
				Cui.CreateIcon(main, $"{0.5f + pipewidth - conticonsize} {pipey - conticonsize}", $"{0.5f + pipewidth + conticonsize} {pipey + conticonsize}", destContainerIconUrl)
			);
			
			float separator = 0.25f;
			float separatorright = 0.75f;
			float gap = 0.015f;
			float lineheight = 0.07f;

			Dictionary<string, string> info = GetMenuInfo(userInfo);

			float topheight = 0.075f;
			//float infoy = (info.Count * lineheight) * 0.5f + 0.03f + topheight;
			float infoyoffset = 0.33f;
			float infoy = (info.Count * lineheight) * 0.5f + 0.03f + topheight + infoyoffset;

			string infobg = elements.Add(
				new CuiPanel {
					Image = { Color = "0.251 0.769 1 0.25" },
					RectTransform = { AnchorMin = $"0 {infoyoffset}", AnchorMax = $"0.996 {infoy}" },
				}, main
			);

			// info top drop shadow
			elements.Add(
				new CuiPanel {
					Image = { Color = "0.004 0.341 0.608 0.15" },
					RectTransform = { AnchorMin = $"0 {infoy - topheight - 0.008f}", AnchorMax = $"0.996 {infoy}" },
				}, main
			);

			elements.Add(
				new CuiPanel {
					Image = { Color = "0.004 0.341 0.608 0.15" },
					RectTransform = { AnchorMin = $"0 {infoy - topheight - 0.016f}", AnchorMax = $"0.996 {infoy}" },
				}, main
			);

			// info top area
			string infotop = elements.Add(
				new CuiPanel {
					Image = { Color = "0.251 0.769 1 0.2" },
					RectTransform = { AnchorMin = $"0 {infoy - topheight}", AnchorMax = $"0.996 {infoy}" },
				}, main
			);

			elements.Add(
				new CuiLabel {
					Text = { Text = $"INFORMATION", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "1 1 1 0.75" },
					RectTransform = { AnchorMin = "0.03 0", AnchorMax = "1 1" }
				}, infotop
			);


			// Deployable Info
			// left
			for (int i = 0; i < info.Count; i+=2) {

				elements.Add(
					new CuiLabel {
						Text = { Text = info.Keys.ElementAt(i), FontSize = 12, Align = TextAnchor.MiddleRight, Color = "1 1 1 0.5" },
						RectTransform = { AnchorMin = $"0 {infoy - topheight - gap - ((lineheight * 0.5f) * i) - lineheight}", AnchorMax = $"{separator - gap} {infoy - topheight - gap - ((lineheight * 0.5f) * i)}" }
					}, main
				);
				elements.Add(
					new CuiLabel {
						Text = { Text = info.Values.ElementAt(i), FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "1 1 1 0.9" },
						RectTransform = { AnchorMin = $"{separator + gap} {infoy - topheight - gap - ((lineheight * 0.5f) * i) - lineheight}", AnchorMax = $"1 {infoy - topheight - gap - ((lineheight * 0.5f) * i)}" }
					}, main
				);
			}
			// right
			for (int i = 1; i < info.Count; i+=2) {

				elements.Add(
					new CuiLabel {
						Text = { Text = info.Keys.ElementAt(i), FontSize = 12, Align = TextAnchor.MiddleRight, Color = "1 1 1 0.5" },
						RectTransform = { AnchorMin = $"0 {infoy - topheight - gap - ((lineheight * 0.5f) * (i - 1)) - lineheight}", AnchorMax = $"{separatorright - gap} {infoy - topheight - gap - ((lineheight * 0.5f) * (i - 1))}" }
					}, main
				);
				elements.Add(
					new CuiLabel {
						Text = { Text = info.Values.ElementAt(i), FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "1 1 1 0.9" },
						RectTransform = { AnchorMin = $"{separatorright + gap} {infoy - topheight - gap - ((lineheight * 0.5f) * (i - 1)) - lineheight}", AnchorMax = $"1 {infoy - topheight - gap - ((lineheight * 0.5f) * (i - 1))}" }
					}, main
				);
			}
		}

		public override Dictionary<string, string> GetMenuInfo(UserInfo userInfo) {
			Dictionary<string, string> info = base.GetMenuInfo(userInfo);

			info.Add("Flowrate", isWaterPipe ? $"{flowrate} ml/sec" : $"{flowrate} items/sec");
			info.Add("Length", Math.Round(distance, 2).ToString());
			
			return info;
		}

		public override List<Cui.ButtonInfo> GetMenuButtons(UserInfo userInfo) {
			List<Cui.ButtonInfo> buttons = new List<Cui.ButtonInfo> {
				new Cui.ButtonInfo("Change Direction", "changedir")
			};
			if (data.Get("grade") != "0") {	// if not twig
				buttons.Add(new Cui.ButtonInfo("Auto Starter", "autostarter", bool.Parse(data.Get("autostart", "false")), destisstartable ? Cui.ButtonInfo.ButtonState.Enabled : Cui.ButtonInfo.ButtonState.Disabled));
				buttons.Add(new Cui.ButtonInfo(ModeNames[(int) mode], "mode"));
			}
			if (itemfiltersizes[int.Parse(data.Get("grade"))] > 0) // if filtersize isn't 0
				buttons.Add(new Cui.ButtonInfo("Item Filter", "filter", mode == Mode.Fueling ? Cui.ButtonInfo.ButtonState.Disabled : Cui.ButtonInfo.ButtonState.Enabled));

			return buttons;
		}

		public override void MenuButtonCallback(UserInfo player, string value) {

			if (value == "changedir") {
				ChangeDirection();
			} else if (value == "mode") {
				NextMode();
			} else if (value == "autostarter") {
				ToggleAutoStarter();
			}
		}

		#endregion
	}
}