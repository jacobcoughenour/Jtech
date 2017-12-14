using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Game.Rust.Cui;

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

		public static class Menu {

			public static string CreateOverlay(CuiElementContainer elements) {

				string parent = elements.Add(
					new CuiPanel {
						Image = { Color = "0.004 0.341 0.608 0.86" },
						RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
						CursorEnabled = true
					}
				);

				elements.Add(
					new CuiButton {
						Button = { Command = $"jtech.closeoverlay", Color = "0 0 0 0" },
						RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
						Text = { Text = string.Empty }
					}, parent
				);

				elements.Add(
					new CuiButton {
						Button = { Command = "", Color = "1 1 0 0.2" },
						RectTransform = { AnchorMin = "0.4 0.45", AnchorMax = "0.48 0.55" },
						Text = { Text = "hi", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 0.5" }
					}, parent
				);
				
				return parent;
			}

		}
	}

}