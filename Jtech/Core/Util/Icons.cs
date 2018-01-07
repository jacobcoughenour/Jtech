using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using System.Linq;

namespace Oxide.Plugins.JtechCore.Util {

	public static class Icons {

		// list of items with shortnames: https://github.com/OxideMod/Oxide.Docs/blob/master/source/includes/rust/item_list.md
		// item icons: http://rust.wikia.com/wiki/Items

		private readonly static Dictionary<string, string> ItemUrls = new Dictionary<string, string>() {
			// the spacing between means that there are items missing from the list there
			// TODO get more item icons

			{ "autoturret", "http://vignette2.wikia.nocookie.net/play-rust/images/f/f9/Auto_Turret_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "bbq", "https://vignette.wikia.nocookie.net/play-rust/images/f/f8/Barbeque_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "box.repair.bench", "http://vignette1.wikia.nocookie.net/play-rust/images/3/3b/Repair_Bench_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "box.wooden", "http://vignette2.wikia.nocookie.net/play-rust/images/f/ff/Wood_Storage_Box_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "box.wooden.large", "http://vignette1.wikia.nocookie.net/play-rust/images/b/b2/Large_Wood_Box_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "campfire", "http://vignette4.wikia.nocookie.net/play-rust/images/3/35/Camp_Fire_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "ceilinglight", "http://vignette3.wikia.nocookie.net/play-rust/images/4/43/Ceiling_Light_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "cupboard.tool", "http://vignette2.wikia.nocookie.net/play-rust/images/5/57/Tool_Cupboard_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "dropbox", "http://vignette2.wikia.nocookie.net/play-rust/images/4/46/Drop_Box_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "fishtrap.small", "http://vignette2.wikia.nocookie.net/play-rust/images/9/9d/Survival_Fish_Trap_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "flameturret", "http://vignette2.wikia.nocookie.net/play-rust/images/f/f9/Flame_Turret_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "fridge", "http://vignette2.wikia.nocookie.net/play-rust/images/8/88/Fridge_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "furnace", "http://vignette4.wikia.nocookie.net/play-rust/images/e/e3/Furnace_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "furnace.large", "http://vignette3.wikia.nocookie.net/play-rust/images/e/ee/Large_Furnace_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "gears", "https://vignette.wikia.nocookie.net/play-rust/images/7/72/Gears_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "guntrap", "http://vignette2.wikia.nocookie.net/play-rust/images/6/6c/Shotgun_Trap_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "hammer", "http://vignette4.wikia.nocookie.net/play-rust/images/5/57/Hammer_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "jackolantern.angry", "http://vignette4.wikia.nocookie.net/play-rust/images/9/96/Jack_O_Lantern_Angry_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "jackolantern.happy", "http://vignette1.wikia.nocookie.net/play-rust/images/9/92/Jack_O_Lantern_Happy_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "lantern", "http://vignette4.wikia.nocookie.net/play-rust/images/4/46/Lantern_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "metal.fragments", "https://vignette.wikia.nocookie.net/play-rust/images/7/74/Metal_Fragments_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "metal.refined", "https://vignette.wikia.nocookie.net/play-rust/images/a/a1/High_Quality_Metal_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "mining.pumpjack", "http://vignette2.wikia.nocookie.net/play-rust/images/c/c9/Pump_Jack_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "mining.quarry", "http://vignette1.wikia.nocookie.net/play-rust/images/b/b8/Mining_Quarry_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "planter.large", "http://vignette1.wikia.nocookie.net/play-rust/images/3/35/Large_Planter_Box_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "planter.small", "http://vignette3.wikia.nocookie.net/play-rust/images/a/a7/Small_Planter_Box_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "recycler", "http://vignette2.wikia.nocookie.net/play-rust/images/e/ef/Recycler_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "research.table", "http://vignette2.wikia.nocookie.net/play-rust/images/2/21/Research_Table_icon.png/revision/latest/scale-to-width-down/{0}" },
			
			{ "scrap", "https://vignette.wikia.nocookie.net/play-rust/images/0/03/Scrap_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "searchlight", "http://vignette2.wikia.nocookie.net/play-rust/images/c/c6/Search_Light_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "small.oil.refinery", "http://vignette2.wikia.nocookie.net/play-rust/images/a/ac/Small_Oil_Refinery_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "stash.small", "http://vignette2.wikia.nocookie.net/play-rust/images/5/53/Small_Stash_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "stocking.large", "http://vignette1.wikia.nocookie.net/play-rust/images/6/6a/SUPER_Stocking_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "stocking.small", "http://vignette2.wikia.nocookie.net/play-rust/images/9/97/Small_Stocking_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "vending.machine", "http://vignette2.wikia.nocookie.net/play-rust/images/5/5c/Vending_Machine_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "wall.frame.shopfront", "http://vignette4.wikia.nocookie.net/play-rust/images/c/c1/Shop_Front_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "water.barrel", "http://vignette4.wikia.nocookie.net/play-rust/images/e/e2/Water_Barrel_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "water.catcher.large", "http://vignette2.wikia.nocookie.net/play-rust/images/3/35/Large_Water_Catcher_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "water.catcher.small", "http://vignette2.wikia.nocookie.net/play-rust/images/0/04/Small_Water_Catcher_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "water.purifier", "http://vignette3.wikia.nocookie.net/play-rust/images/6/6e/Water_Purifier_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "wood", "https://vignette.wikia.nocookie.net/play-rust/images/f/f2/Wood_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "xmas.present.large", "http://vignette1.wikia.nocookie.net/play-rust/images/9/99/Large_Present_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "xmas.present.medium", "http://vignette3.wikia.nocookie.net/play-rust/images/6/6b/Medium_Present_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "xmas.present.small", "http://vignette2.wikia.nocookie.net/play-rust/images/d/da/Small_Present_icon.png/revision/latest/scale-to-width-down/{0}" },

		};


		public static string GetItemIconURL(string name, int size) {
			string url;
			if (ItemUrls.TryGetValue(name, out url)) {
				return string.Format(url, size);
			}
			return string.Empty;
		}

		public static string GetContainerIconURL(BaseEntity e, int size) {

			// TODO if e is JDeployable.Child, get JDeployable url
			// TODO add more containers

			var c = e.GetComponent<JDeployable.Child>();
			if (c != null) {
				JInfoAttribute info;
				if (JDeployableManager.DeployableTypes.TryGetValue(c.parent.GetType(), out info))
					return info.IconUrl;

			} else if (e is BoxStorage) {
				string panel = e.GetComponent<StorageContainer>().panelName;
				if (panel == "largewoodbox")
					return GetItemIconURL("box.wooden.large", size);
				return GetItemIconURL("box.wooden", size);

			} else if (e is BaseOven) {
				string panel = e.GetComponent<BaseOven>().panelName;

				if (panel == "largefurnace")
					return GetItemIconURL("furnace.large", size);
				else if (panel == "smallrefinery")
					return GetItemIconURL("small.oil.refinery", size);
				else if (panel == "lantern")
					return GetItemIconURL("lantern", size);
				else if (panel == "bbq")
					return GetItemIconURL("bbq", size);
				else if (panel == "campfire")
					return GetItemIconURL("campfire", size);
				else
					return GetItemIconURL("furnace", size);
			} else if (e is AutoTurret) {
				return GetItemIconURL("autoturret", size);
			} else if (e is Recycler) {
				return GetItemIconURL("recycler", size);
			} else if (e is FlameTurret) {
				return GetItemIconURL("flameturret", size);
			} else if (e is GunTrap) {
				return GetItemIconURL("guntrap", size);
			} else if (e is SearchLight) {
				return GetItemIconURL("searchlight", size);
			} else if (e is WaterCatcher) {
				if (e.GetComponent<WaterCatcher>()._collider.ToString().Contains("small"))
					return GetItemIconURL("water.catcher.small", size);
				return GetItemIconURL("water.catcher.large", size);
			} else if (e is LiquidContainer) {
				if (e.GetComponent<LiquidContainer>()._collider.ToString().Contains("purifier"))
					return GetItemIconURL("water.purifier", size);
				return GetItemIconURL("water.barrel", size);
			} else if (e is VendingMachine) {
				return GetItemIconURL("vending.machine", size);
			} else if (e is DropBox) {
				return GetItemIconURL("dropbox", size);
			} else if (e is StashContainer) {
				return GetItemIconURL("stash.small", size);
			} else if (e is MiningQuarry) {
				if (e.ToString().Contains("pump"))
					return GetItemIconURL("mining.pumpjack", size);
				return GetItemIconURL("mining.quarry", size);
			} else if (e is BuildingPrivlidge) {
				return GetItemIconURL("cupboard.tool", size);
			}

			return "http://i.imgur.com/BwJN0rt.png";
		}

	}

}