using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Oxide.Core.Plugins;
using Oxide.Plugins.JTechCore;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;
using System.Linq;
using System.Text;

namespace Oxide.Plugins {

    [Info("JTech", "TheGreatJ", "1.0.0", ResourceId = 2402)]
    class JTech : RustPlugin {

        [PluginReference]
        private Plugin FurnaceSplitter;

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
			
		}

		void Unload() {

			// TODO
			// save deployables
			// unload deployables

			// Destroy UserInfo from all the players
			var users = UnityEngine.Object.FindObjectsOfType<UserInfo>();
			if (users != null) {
				foreach (var go in users) {
					if (!string.IsNullOrEmpty(go.overlay))
						CuiHelper.DestroyUi(go.player, go.overlay);
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

		void OnPlayerInit(BasePlayer player) {
			// Add UserInfo to player
			UserInfo.Get(player);
		}

		#endregion

		#region Structure

		void OnHammerHit(BasePlayer player, HitInfo hit) {
			// TODO
			// open menu if deployable
		}

		#endregion


		#endregion

		
		[ConsoleCommand("jtech.closeoverlay")]
		private void closeoverlay(ConsoleSystem.Arg arg) {
			UserInfo.Get(arg.Player()).HideOverlay();
		}

	}
}

namespace Oxide.Plugins.JTechCore {

	public class BaseDeployable {

		// TODO
		// handle entity hooks (health, damage, repair)
		// owner, parent
		// cui menu
		// spawn/destroy 
		// load/save


		public BaseDeployable() { }

		

	}

	// component added to BaseEntity(s) for handling hooks
	public class BaseDeployableChild : MonoBehaviour {
		// TODO
	}

	// component added to parent for handling hooks
	public class BaseDeployableBehaviour : BaseDeployableChild {
		// TODO
	}

}

namespace Oxide.Plugins.JTechCore {

	public static class Cui {

		public static CuiLabel CreateLabel(string text, int i, float rowHeight, TextAnchor align = TextAnchor.MiddleLeft, int fontSize = 15, string xMin = "0", string xMax = "1", string color = "1.0 1.0 1.0 1.0") {
			return new CuiLabel {
				Text = { Text = text, FontSize = fontSize, Align = align, Color = color },
				RectTransform = { AnchorMin = $"{xMin} {1 - rowHeight * i + i * .002f}", AnchorMax = $"{xMax} {1 - rowHeight * (i - 1) + i * .002f}" }
			};
		}

		public static CuiButton CreateButton(string command, float i, float rowHeight, int fontSize = 15, string content = "+", string xMin = "0", string xMax = "1", string color = "0.8 0.8 0.8 0.2", string textcolor = "1 1 1 1", float offset = -.005f) {
			return new CuiButton {
				Button = { Command = command, Color = color },
				RectTransform = { AnchorMin = $"{xMin} {1 - rowHeight * i + i * offset}", AnchorMax = $"{xMax} {1 - rowHeight * (i - 1) + i * offset}" },
				Text = { Text = content, FontSize = fontSize, Align = TextAnchor.MiddleCenter, Color = textcolor }
			};
		}

		public static CuiPanel CreatePanel(string anchorMin, string anchorMax, string color = "0 0 0 0") {
			return new CuiPanel {
				Image = { Color = color },
				RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax }
			};
		}

		public static CuiElement CreateInputField(string parent = "Hud", string command = "", string text = "", int fontsize = 14, int charlimit = 100, string name = null) {

			if (string.IsNullOrEmpty(name))
				name = CuiHelper.GetGuid();
			CuiElement cuiElement = new CuiElement();
			cuiElement.Name = name;
			cuiElement.Parent = parent;
			cuiElement.Components.Add((ICuiComponent) new CuiInputFieldComponent { Text = "he", Align = TextAnchor.MiddleCenter, CharsLimit = charlimit, Command = command, FontSize = fontsize });
			cuiElement.Components.Add((ICuiComponent) new CuiNeedsCursorComponent());

			return cuiElement;
		}

		public static CuiElement AddOutline(CuiLabel label, string parent = "Hud", string color = "0.15 0.15 0.15 0.43", string dist = "1.1 -1.1", bool usealpha = false, string name = null) {
			if (string.IsNullOrEmpty(name))
				name = CuiHelper.GetGuid();
			CuiElement cuiElement = new CuiElement();
			cuiElement.Name = name;
			cuiElement.Parent = parent;
			cuiElement.FadeOut = label.FadeOut;
			cuiElement.Components.Add((ICuiComponent) label.Text);
			cuiElement.Components.Add((ICuiComponent) label.RectTransform);
			cuiElement.Components.Add((ICuiComponent) new CuiOutlineComponent {
				Color = color,
				Distance = dist,
				UseGraphicAlpha = usealpha
			});
			return cuiElement;
		}

		public static CuiElement CreateItemIcon(string parent = "Hud", string anchorMin = "0 0", string anchorMax = "1 1", string imageurl = "", string color = "1 1 1 1") => new CuiElement {
			Parent = parent,
			Components = {
				new CuiRawImageComponent {
					Url = imageurl,
					Sprite = "assets/content/textures/generic/fulltransparent.tga",
					Color = color
				},
				new CuiRectTransformComponent {
					AnchorMin = anchorMin,
					AnchorMax = anchorMax
				},
			}
		};
	}

}

namespace Oxide.Plugins.JTechCore {

	public static class Data {

		public static void Load() {
			// TODO
		}

		public static void Save() {
			// TODO
		}

		private static void LoadData<T>(ref T data) => data = Core.Interface.Oxide.DataFileSystem.ReadObject<T>("JTech");
		private static void SaveData<T>(T data) => Core.Interface.Oxide.DataFileSystem.WriteObject("JTech", data);
	}
}

namespace Oxide.Plugins.JTechCore {

	public class DeployableManager {
		
		private Dictionary<ulong, BaseDeployable> registeredDeployables = new Dictionary<ulong, BaseDeployable>();

		// TODO
		// manage spawned deployables
		// distributive deployable update
		// load deployable types
		// load and spawn deployables from save file (async)
		// save deployables
		// clean up deployables on unload




	}
}

namespace Oxide.Plugins.JTechCore {

	class UserInfo : MonoBehaviour {

		public BasePlayer player;
		public InputState input;
		private bool isHoldingHammer;
		private bool isDown;
		private uint lastActiveItem;
		private bool isOverlayOpen;
		private float startPressingTime;

		public string overlay;

		void Awake() {

			player = GetComponent<BasePlayer>();
			input = player.serverInput;
			enabled = true;
			lastActiveItem = 0;
			isOverlayOpen = false;
		}

		void Update() {

			if (player.svActiveItemID != lastActiveItem) {
				OnPlayerActiveItemChanged();
				lastActiveItem = player.svActiveItemID;
			}

			if (!isOverlayOpen && isHoldingHammer) {
				if (input.WasJustPressed(BUTTON.FIRE_SECONDARY) && !isDown) {
					startPressingTime = Time.realtimeSinceStartup;
					isDown = true;
				} else if (input.IsDown(BUTTON.FIRE_SECONDARY)) {
					if ((Time.realtimeSinceStartup - startPressingTime) > 0.2f) {
						ShowOverlay();
						isDown = false;
					}
				} else {
					isDown = false;
				}
			}
			
		}

		private void OnPlayerActiveItemChanged() {
			var item = player.GetActiveItem();
			isHoldingHammer = (item != null && item.info != null && (item.info.name == "hammer.item"));
		}

		private static string subtext = "subtext";
		private static string text = "text";
		private static string textcolor = "1 1 1 0.5";

		public void ShowOverlay() {
			HideOverlay(); // just in case
			
			var elements = new CuiElementContainer();

			overlay = elements.Add(
				new CuiPanel {
					Image = { Color = "0.004 0.341 0.608 0.86" },
					RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
					CursorEnabled = true
				}
			);

			//elements.Add(
			//	Cui.AddOutline(
			//	new CuiLabel {
			//		Text = { Text = "Jtech", FontSize = 28, Align = TextAnchor.MiddleCenter, Color = textcolor },
			//		RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
			//	},
			//	overlay)
			//);

			elements.Add(
				new CuiButton {
					Button = { Command = $"jtech.closeoverlay", Color = "0 0 0 0" },
					RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
					Text = { Text = string.Empty }
				}, overlay
			);

			elements.Add(
				new CuiButton {
					Button = { Command = "", Color = "1 1 0 0.2" },
					RectTransform = { AnchorMin = "0.4 0.45", AnchorMax = "0.48 0.55" },
					Text = { Text = "hi", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = textcolor }
				}, overlay
			);

			CuiHelper.AddUi(player, elements);

			//overlaytext = text;
			//overlaysubtext = subtext;
			isOverlayOpen = true;
		}

		public void HideOverlay() {
			if (!string.IsNullOrEmpty(overlay))
				Game.Rust.Cui.CuiHelper.DestroyUi(player, overlay);
			isOverlayOpen = false;
		}


		/// <summary>
		/// Get/create UserInfo from a BasePlayer.
		/// </summary>
		public static UserInfo Get(BasePlayer basePlayer) {
			return basePlayer.GetComponent<UserInfo>() ?? basePlayer.gameObject.AddComponent<UserInfo>();
		}

	}

}

namespace Oxide.Plugins.JTechCore.Deployables {

	public class Pipe : BaseDeployable {

		// TODO

	}
}
