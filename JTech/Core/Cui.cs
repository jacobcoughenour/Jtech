using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using System.Linq;
using System.Text;

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

		public static void FakeDropShadow(CuiElementContainer elements, string parent = "Hud", float anchorMinx = 0, float anchorMiny = 0, float anchorMaxx = 1, float anchorMaxy = 1, float widthseparation = 0.025f, float heightseparation = 0.025f, int dist = 3, string color = "0.15 0.15 0.15 0.1") {

			for (var i = 1; i <= dist; i++)
				elements.Add(
					new CuiPanel {
						Image = { Color = color },
						RectTransform = { AnchorMin = $"{anchorMinx - widthseparation * i} {anchorMiny - heightseparation * i}", AnchorMax = $"{anchorMaxx + widthseparation * i} {anchorMaxy + heightseparation * i}" }
					}, parent
				);
		}

		//private static char[] bglut = new char[] {' ','.',':','-','=','+','*','#','%','@'};
		//private static char[] bglut = "@@@@@@@######MMMBBHHHAAAA&&GGhh9933XXX222255SSSiiiissssrrrrrrr;;;;;;;;:::::::,,,,,,,........".ToCharArray();
		//private static char[] bglut = "@MBHENR#KWXDFPQASUZbdehx*8Gm&04LOVYkpq5Tagns69owz$CIu23Jcfry%1v7l+it[] {}?j|()=~!-/<>\"^_';,:`.".ToCharArray();

		//public static string ASCIIbg(int width, int height, float scalex, float scaley, Color white, Color black) {

		//	List<string> lines = new List<string>();
		//	for (int y = 0; y < height; y++) {
		//		string line = "";
		//		for (int x = 0; x < width; x++) {
		//			//int r = Mathf.FloorToInt(Mathf.Clamp(UnityEngine.Mathf.PerlinNoise(x / scalex, y / scaley) * bglut.Length, 0, bglut.Length - 1));
		//			float r = UnityEngine.Mathf.PerlinNoise(x / scalex, y / scaley);

		//			UnityEngine.Color color = Color.Lerp(black, white, r*0.5f);

		//			//line += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}> × </color>";
		//			line += " × ";
		//		}
		//		lines.Add(line);
		//	}
		//	return string.Join("", lines.ToArray());
		//	//return string.Join("\n", lines.ToArray());
		//}

		public static class Menu {

			public static string CreateOverlay(CuiElementContainer elements, UserInfo userInfo) {

				List<Type> registeredDeployables = JDeployableManager.DeployableTypes.Keys.ToList<Type>();

				float aspect = 0.5625f; // use this to scale width values for 1:1 aspect
				
				float buttonsize = 0.16f;
				float buttonsizeaspect = buttonsize * aspect;
				float buttonspacing = 0.04f * aspect;
				int numofbuttons = registeredDeployables.Count;
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
						}, parent, "0.004 0.341 0.608 0.6")
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

					Type currenttype = registeredDeployables[i];
					JInfoAttribute info;
					JDeployableManager.DeployableTypes.TryGetValue(currenttype, out info);
					List<JRequirementAttribute> requirements;
					JDeployableManager.DeployableTypeRequirements.TryGetValue(currenttype, out requirements);

					bool canCraftDeployable = userInfo.CanCraftDeployable(currenttype);

					int ix = i % maxbuttonswrap;
					int iy = i/maxbuttonswrap;
					
					float posx = 0.5f + ((ix - (numofbuttons * 0.5f)) * (buttonsizeaspect + buttonspacing)) + buttonspacing * 0.5f;
					float posy = 0.55f - (buttonsize * 0.5f) - (iy * ((buttonsize) + buttonspacing*2));

					// slight outline around the button
					FakeDropShadow(elements, parent, posx, posy - buttonsize*0.5f, posx + buttonsizeaspect, posy + (buttonsize), 0.005f*aspect, 0.005f, 1, "0.004 0.341 0.608 0.1");

					// main button
					string button = elements.Add(
						new CuiButton {
							Button = { Command = canCraftDeployable ? $"jtech.startplacing {currenttype.FullName}" : "", Color = canCraftDeployable ? "0.251 0.769 1 0.25" : "0.749 0.922 1 0.075" },
							RectTransform = { AnchorMin = $"{posx} {posy - buttonsize * 0.5f}", AnchorMax = $"{posx + buttonsizeaspect} {posy + (buttonsize)}" },
							Text = { Text = "", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 0" }
						}, parent
					);

					// deployable icon
					elements.Add(
						CreateItemIcon(button, "0.05 0.383", "0.95 0.95", info.IconUrl, canCraftDeployable ? "1 1 1 1" : "0.749 0.922 1 0.5")
					);

					// button bottom area
					string buttonbottom = elements.Add(
						new CuiPanel {
							Image = { Color = "0 0 0 0" },
							RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.3333" }
						}, button
					);

					// deployable name label shadow
					FakeDropShadow(elements, buttonbottom, 0, 0.6f, 1, 1f, 0, 0.02f, 2, "0.004 0.341 0.608 0.15");

					// deployable name label
					string buttonlabel = elements.Add(
						new CuiPanel {
							Image = { Color = canCraftDeployable ? "0.251 0.769 1 0.9" : "0.749 0.922 1 0.3" },
							RectTransform = { AnchorMin = "-0.031 0.6", AnchorMax = "1.0125 1" }
						}, buttonbottom
					);

					// deployable name label text
					elements.Add(
						AddOutline(
						new CuiLabel {
							Text = { Text = info.Name, FontSize = 16, Align = TextAnchor.MiddleCenter, Color = canCraftDeployable ? "1 1 1 1" : "1 1 1 0.6" },
							RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
						}, buttonlabel, "0.004 0.341 0.608 0.3")
					);

					// item requirements area
					string materiallist = elements.Add(
						new CuiPanel {
							Image = { Color = "0 0 0 0" },
							RectTransform = { AnchorMin = "0 0.1", AnchorMax = "0.9815 0.45" }
						}, buttonbottom
					);


					// item requirements

					int numofrequirements = requirements.Count;
					for (int r = 0; r < numofrequirements; r++) {

						JRequirementAttribute cur = requirements[r];

						bool hasRequirement = userInfo.DoesHaveUsableItem(cur.ItemId, cur.ItemAmount);

						float pos = 0.6f - (numofrequirements*0.1f) + r*(0.2f) - (cur.PerUnit != string.Empty ? cur.PerUnit.Length*0.026f + 0.09f : 0);
						string min = $"{pos - 0.1f} 0";
						string max = $"{pos + 0.1f} 1";
						
						// item icon
						elements.Add(
							CreateItemIcon(materiallist, min, max, Util.Icons.GetItemIconURL(cur.ItemShortName, 64), hasRequirement ? "1 1 1 1" : "1 1 1 0.5")
						);
						
						// item amount
						if (cur.ItemAmount > 1) {
							elements.Add(
								AddOutline(
								new CuiLabel {
									Text = { Text = $"{cur.ItemAmount}", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = hasRequirement ? "1 1 1 1" : "1 0.835 0.31 1" },
									RectTransform = { AnchorMin = min, AnchorMax = max }
								}, materiallist, "0.004 0.341 0.608 0.3")
							);
						}

						// per unit
						if (cur.PerUnit != string.Empty) {
							elements.Add(
								AddOutline(
								new CuiLabel {
									Text = { Text = $"per {cur.PerUnit}", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "1 1 1 1" },
									RectTransform = { AnchorMin = $"{pos + 0.135f} 0", AnchorMax = $"{pos + 1.0f} 1" }
								}, materiallist, "0.004 0.341 0.608 0.3")
							);
						}
					}

				}
				

				return parent;
			}

			public static string CreateMenu(CuiElementContainer elements, UserInfo userInfo, JDeployable deployable) {
				
				JInfoAttribute info;
				JDeployableManager.DeployableTypes.TryGetValue(deployable.GetType(), out info);

				float aspect = 0.5625f; // use this to scale width values for 1:1 aspect

				float mainheight = 0.45f;
				float mainwidth = 0.55f;
				float mainwidthaspect = mainwidth * aspect;
				float mainaspect = mainheight / mainwidth;
				float mainy = 0.475f;

				string parent = elements.Add(
					new CuiPanel { // blue background
						Image = { Color = "0.004 0.341 0.608 0.86" },
						RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
						CursorEnabled = true
					}
				);

				//elements.Add(
				//	new CuiLabel {
				//		Text = { Text = new StringBuilder().Insert(0, "▀", 10000).ToString(), FontSize = 6, Align = TextAnchor.MiddleCenter, Color = "1 1 1 0.5" },
				//		RectTransform = { AnchorMin = $"{0.5f - mainwidthaspect * 0.5f} {mainy - mainheight * 0.5f}", AnchorMax = $"{0.5f + mainwidthaspect * 0.5f} {mainy + mainheight * 0.5f}" }
				//	}, parent
				//);

				// close overlay if you click the background
				elements.Add(
					new CuiButton {
						Button = { Command = $"jtech.closemenu", Color = "0 0 0 0" },
						RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
						Text = { Text = string.Empty }
					}, parent
				);

				//return parent;

				float gap = 0.0125f;

				float centercontentheight = 0.8f;
				float centercontentwidth = centercontentheight * mainaspect; // keep the main content square

				string main = elements.Add(
					new CuiPanel {
						Image = { Color = "0.004 0.341 0.608 0" },
						RectTransform = { AnchorMin = $"{0.5f - mainwidthaspect * 0.5f} {mainy - mainheight * 0.5f}", AnchorMax = $"{0.5f + mainwidthaspect * 0.5f} {mainy + mainheight * 0.5f}" },
					}, parent
				);

				// slight outline around main
				FakeDropShadow(elements, parent, 0.5f - mainwidthaspect * 0.5f, mainy - mainheight * 0.5f, 0.5f + mainwidthaspect * 0.5f, mainy + mainheight * 0.5f, 0.005f * aspect, 0.005f, 1, "0.004 0.341 0.608 0.2");



				// top

				string top = elements.Add(
					new CuiPanel {
						Image = { Color = "0.251 0.769 1 0.25" },
						RectTransform = { AnchorMin = $"0 {centercontentheight + gap}", AnchorMax = $"1 1" },
					}, main
				);

				// deployable name
				elements.Add(
					AddOutline(
					new CuiLabel {
						Text = { Text = $"{info.Name}", FontSize = 24, Align = TextAnchor.LowerLeft, Color = "1 1 1 1" },
						RectTransform = { AnchorMin = "0 1", AnchorMax = "1 2" }
					}, top, "0.004 0.341 0.608 0.6")
				);

				float topaspect = (1 - centercontentheight + gap / 1) * mainaspect - 0.0025f;
				float iconwidth = topaspect;
				float iconpadding = 0.03f;
				float textpadding = 0.125f;
				float iconleft = 1 - iconwidth - 0.02f;

				// deployable description
				elements.Add(
					AddOutline(
					new CuiLabel {
						Text = { Text = info.Description, FontSize = 12, Align = TextAnchor.UpperLeft, Color = "1 1 1 1" },
						RectTransform = { AnchorMin = $"{textpadding * topaspect} {textpadding}", AnchorMax = $"{iconleft - (textpadding * topaspect)} {1f - textpadding}" }
					}, top, "0.004 0.341 0.608 0.8")
				);

				// deployable icon
				string iconarea = elements.Add(
					new CuiPanel {
						Image = { Color = "0 0 0 0" },
						RectTransform = { AnchorMin = $"{iconleft} 0", AnchorMax = $"{iconleft + iconwidth - (0.02f * topaspect)} {0.98f}" },
					}, top
				);

				elements.Add(
					CreateItemIcon(iconarea, $"{iconpadding} {iconpadding}", $"{1 - iconpadding} {1 - iconpadding}", info.IconUrl, "1 1 1 1")
				);




				// content	

				string content = elements.Add(
					new CuiPanel {
						Image = { Color = "0.251 0.769 1 0.25" },
						RectTransform = { AnchorMin = "0 0", AnchorMax = $"{centercontentwidth} {centercontentheight}" },
					}, main
				);


				// actions

				string actions = elements.Add(
					new CuiPanel {
						Image = { Color = "0 1 0 0" },
						RectTransform = { AnchorMin = $"{centercontentwidth + gap * mainaspect} 0", AnchorMax = $"1 {centercontentheight}" },
					}, main
				);



				return parent;
			}

		}
	}

}