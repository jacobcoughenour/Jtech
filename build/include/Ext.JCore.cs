using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using System.Linq;
using System.Text;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Core;
using Oxide.Plugins;
using System.Reflection;
using Oxide.Ext.JCore;

namespace Oxide.Ext.JCore {

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

		public static void FakeDropShadow(CuiElementContainer elements, string parent = "Hud", float anchorMinx = 0, float anchorMiny = 0, float anchorMaxx = 1, float anchorMaxy = 1, float widthseparation = 0.025f, float heightseparation = 0.025f, int dist = 3, string color = "0.15 0.15 0.15 0.1") {

			for (var i = 1; i <= dist; i++)
				elements.Add(
					new CuiPanel {
						Image = { Color = color },
						RectTransform = { AnchorMin = $"{anchorMinx - widthseparation * i} {anchorMiny - heightseparation * i}", AnchorMax = $"{anchorMaxx + widthseparation * i} {anchorMaxy + heightseparation * i}" }
					}, parent
				);
		}
		

		public static class Menu {

			public static string CreateOverlay(CuiElementContainer elements) {

				float aspect = 0.5625f; // use this to scale width values for 1:1 aspect
				
				float buttonsize = 0.16f;
				float buttonsizeaspect = buttonsize * aspect;
				float buttonspacing = 0.04f * aspect;
				int numofbuttons = 4;
				int maxbuttonswrap = 8;

				string parent = elements.Add(
					new CuiPanel { // blue background
						Image = { Color = "0.004 0.341 0.608 0.86" },
						RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
						CursorEnabled = true
					}
				);

				elements.Add(
					AddOutline(
						new CuiLabel {
							Text = { Text = "Choose a Deployable", FontSize = 22, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
							RectTransform = { AnchorMin = "0 0.5", AnchorMax = "1 1" }
						}, parent)
				);

				// close overlay if you click the background
				elements.Add(
					new CuiButton {
						Button = { Command = $"jtech.closeoverlay", Color = "0 0 0 0" },
						RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
						Text = { Text = string.Empty }
					}, parent
				);

				// create buttons

				for (int i = 0; i < numofbuttons; i++) {

					int ix = i % maxbuttonswrap;
					int iy = i/maxbuttonswrap;
					
					float posx = 0.5f + ((ix - (numofbuttons * 0.5f)) * (buttonsizeaspect + buttonspacing)) + buttonspacing * 0.5f;
					float posy = 0.55f - (buttonsize * 0.5f) - (iy * ((buttonsize) + buttonspacing*2));

					FakeDropShadow(elements, parent, posx, posy - buttonsize*0.5f, posx + buttonsizeaspect, posy + (buttonsize), 0.005f*aspect, 0.005f, 1, "0.004 0.341 0.608 0.1");

					string button = elements.Add(
						new CuiButton {
							Button = { Command = "", Color = "0.251 0.769 1 0.25" },
							RectTransform = { AnchorMin = $"{posx} {posy - buttonsize * 0.5f}", AnchorMax = $"{posx + buttonsizeaspect} {posy + (buttonsize)}" },
							Text = { Text = "", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 0" }
						}, parent
					);

					elements.Add(
						CreateItemIcon(button, "0.05 0.383", "0.95 0.95", "https://i.imgur.com/R9mD3VQ.png", "1 1 1 1")
					);

					string buttonbottom = elements.Add(
						new CuiPanel {
							Image = { Color = "0 0 0 0" },
							RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.3333" }
						}, button
					);

					FakeDropShadow(elements, buttonbottom, 0, 0.6f, 1, 1f, 0, 0.02f, 2, "0.004 0.341 0.608 0.15");

					string buttonlabel = elements.Add(
						new CuiPanel {
							Image = { Color = "0.251 0.769 1 0.9" },
							RectTransform = { AnchorMin = "-0.031 0.6", AnchorMax = "1.0125 1" }
						}, buttonbottom
					);

					elements.Add(
						AddOutline(
						new CuiLabel {
							Text = { Text = "Assembler", FontSize = 16, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
							RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
						}, buttonlabel, "0.004 0.341 0.608 0.3")
					);

					string materiallist = elements.Add(
						new CuiPanel {
							Image = { Color = "0 0 0 0" },
							RectTransform = { AnchorMin = "0 0.05", AnchorMax = "1 0.45" }
						}, buttonbottom
					);


					elements.Add(
						CreateItemIcon(materiallist, "0.2 0", "0.4 1", "https://vignette.wikia.nocookie.net/play-rust/images/5/5c/Vending_Machine_icon.png", "1 1 1 1")
					);
					elements.Add(
						AddOutline(
							new CuiLabel {
								Text = { Text = "", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
								RectTransform = { AnchorMin = "0.2 0", AnchorMax = "0.4 1" }
							}, materiallist, "0.15 0.15 0.15 1")
					);

					elements.Add(
						CreateItemIcon(materiallist, "0.4 0", "0.6 1", "https://vignette.wikia.nocookie.net/play-rust/images/7/72/Gears_icon.png", "1 1 1 1")
					);
					elements.Add(
						AddOutline(
							new CuiLabel {
								Text = { Text = "5", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
								RectTransform = { AnchorMin = "0.4 0", AnchorMax = "0.6 1" }
							}, materiallist, "0.15 0.15 0.15 1")
					);

					elements.Add(
						CreateItemIcon(materiallist, "0.6 0", "0.8 1", "https://vignette.wikia.nocookie.net/play-rust/images/a/a1/High_Quality_Metal_icon.png", "1 1 1 1")
					);
					elements.Add(
						AddOutline(
							new CuiLabel {
								Text = { Text = "20", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
								RectTransform = { AnchorMin = "0.6 0", AnchorMax = "0.8 1" }
							}, materiallist, "0.15 0.15 0.15 1")
					);





				}
				

				return parent;
			}

		}
	}

}

namespace Oxide.Ext.JCore {

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

namespace Oxide.Ext.JCore {

	public class JDeployable {

		// TODO
		// handle entity hooks (health, damage, repair)
		// owner, parent
		// cui menu
		// spawn/destroy 
		// load/save


		public JDeployable() { }
		

	}

}

namespace Oxide.Ext.JCore {

	public class JDeployableManager {

		private static HashSet<Type> DeployableTypes = new HashSet<Type>();
		public static bool UnregisterType<T>() where T : JDeployable => DeployableTypes.Remove(typeof(T));
		
		/// <summary>
		/// JDeployable API
		/// Registers Custom JDeployable to the JDeployableManager
		/// </summary>
		/// <typeparam name="T">Custom Deployable</typeparam>
		public static void RegisterJDeployable<T>() where T : JDeployable {

			// get info attribute
			JInfoAttribute info = (JInfoAttribute) System.Attribute.GetCustomAttribute(typeof(T), typeof(JInfoAttribute));

			if (info != null) {
				if (DeployableTypes.Add(typeof(T)))
					Interface.Oxide.LogInfo($"[JCore] Registered Custom Deployable: [{info.PluginInfo.Title}] {info.Name}");
				else
					Interface.Oxide.LogWarning($"[JCore] ([{info.PluginInfo.Title}] {info.Name}) has already been registered!");
				
			} else
				Interface.Oxide.LogWarning($"[JCore] Failed to register ({typeof(T)}) for Missing JInfo Attribute");

			Interface.Oxide.CallHook("OnRegisterJDeployable", info);
		}
		
		// TODO
		// manage spawned deployables
		// distributive deployable update
		// load deployable types
		// load and spawn deployables from save file (async)
		// save deployables
		// clean up deployables on unload




	}
}

namespace Oxide.Ext.JCore {

	[AttributeUsage(AttributeTargets.Class)]
	public class JInfoAttribute : Attribute {

		public InfoAttribute PluginInfo { get; }
		public string Name { get; }
		public string IconUrl { get; }

		/// <summary>
		/// Info about this Custom JDeployable
		/// </summary>
		/// <param name="pluginType">typeof(yourplugin)</param>
		/// <param name="name">Name shown in menus and commands.</param>
		/// <param name="iconUrl">Url for the icon shown in menus. Make it 200x200 with a transparent background.</param>
		public JInfoAttribute(Type pluginType, string name, string iconUrl) {
			this.PluginInfo = (InfoAttribute) GetCustomAttribute(pluginType, typeof(InfoAttribute));
			this.Name = name;
			this.IconUrl = iconUrl;
		}
	}
}

namespace Oxide.Plugins {

    [Info("JCore", "TheGreatJ", "1.0.0", ResourceId = 2402)]
    class JCore : RustPlugin {

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
				foreach (UserInfo go in users) {
					go.HideOverlay();
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

		void OnPlayerSleepEnded(BasePlayer player) {
			// Add UserInfo to player
			UserInfo.Get(player);
		}

		#endregion

		#region Structure

		//void OnHammerHit(BasePlayer player, HitInfo hit) {
		//	// TODO
		//	// open menu if deployable
		//}

		#endregion


		#endregion

		[ChatCommand("jcore")]
		private void jtechmainchat(BasePlayer player, string cmd, string[] args) {
			UserInfo.ShowOverlay(player);
		}

		[ConsoleCommand("jcore.showoverlay")]
		private void showoverlay(ConsoleSystem.Arg arg) {
			UserInfo.ShowOverlay(arg.Player());
		}

		[ConsoleCommand("jcore.closeoverlay")]
		private void closeoverlay(ConsoleSystem.Arg arg) {
			UserInfo.HideOverlay(arg.Player());
		}

	}
}

namespace Oxide.Ext.JCore {

	public class UserInfo : MonoBehaviour {

		public BasePlayer player;
		public InputState input;

		private bool isHoldingHammer;
		private bool isDown;
		private uint lastActiveItem;
		private float startPressingTime;

		private string overlay; // uid for overlay cui instance
		private bool isOverlayOpen;

		void Awake() {

			player = GetComponent<BasePlayer>();
			input = player.serverInput;
			enabled = true;
			lastActiveItem = 0;
			isOverlayOpen = false;
		}

		void Update() {

			// TODO detect when on a pipe and set violationlevel to 0
			//player.violationLevel = 0;

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

		

		/// <summary>
		/// Show overlay menu for the given BasePlayer
		/// </summary>
		public static void ShowOverlay(BasePlayer basePlayer) => Get(basePlayer).ShowOverlay();

		/// <summary>
		/// Show overlay menu for parent player
		/// </summary>
		public void ShowOverlay() {
			HideOverlay(); // just in case
			
			var elements = new CuiElementContainer();

			overlay = Cui.Menu.CreateOverlay(elements);

			CuiHelper.AddUi(player, elements);

			//overlaytext = text;
			//overlaysubtext = subtext;
			isOverlayOpen = true;
		}

		/// <summary>
		/// Hide overlay menu for the given BasePlayer
		/// </summary>
		public static void HideOverlay(BasePlayer basePlayer) => Get(basePlayer).HideOverlay();

		/// <summary>
		/// Hide overlay menu for parent player
		/// </summary>
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
