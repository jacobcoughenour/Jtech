using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Game.Rust.Cui;

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