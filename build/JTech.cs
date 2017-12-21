using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Oxide.Core.Plugins;
using Oxide.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;
using Oxide.Plugins.JCore;
using System.Linq;
using System.Text;
using Oxide.Core;

namespace Oxide.Plugins {

    [Info("JTech", "TheGreatJ", "1.0.0", ResourceId = 2402)]
    class JTech : RustPlugin {


		void RegisterDeployables() {
			JDeployableManager.RegisterJDeployable<JTechDeployables.TransportPipe>();
			JDeployableManager.RegisterJDeployable<JTechDeployables.Assembler>();
			//JDeployableManager.RegisterJDeployable<JTechDeployables.TrashCan>();
			//JDeployableManager.RegisterJDeployable<JTechDeployables.AutoFarm>();
		}


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


			RegisterDeployables();
		}
		
		void Unload() {

			// TODO
			// save deployables
			// unload deployables

			// Destroy UserInfo from all the players
			var users = UnityEngine.Object.FindObjectsOfType<UserInfo>();
			if (users != null) {
				foreach (UserInfo go in users) {
					go.DestroyCui();
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

		void OnHammerHit(BasePlayer player, HitInfo hit) {
			// TODO
			// open menu if deployable

			UserInfo.OnHammerHit(player, hit);

			//ListComponentsDebug(player, hit.HitEntity);
		}

		#endregion


		#endregion

		[ChatCommand("jtech")]
		private void jtechmainchat(BasePlayer player, string cmd, string[] args) {
			UserInfo.ShowOverlay(player);
		}

		[ConsoleCommand("jtech.showoverlay")]
		private void showoverlay(ConsoleSystem.Arg arg) {
			UserInfo.ShowOverlay(arg.Player());
		}

		[ConsoleCommand("jtech.closeoverlay")]
		private void closeoverlay(ConsoleSystem.Arg arg) {
			UserInfo.HideOverlay(arg.Player());
		}

		[ConsoleCommand("jtech.startplacing")]
		private void startplacing(ConsoleSystem.Arg arg) {

			if (arg.HasArgs()) {

				Type deployabletype;
				if (JDeployableManager.TryGetType(arg.Args[0], out deployabletype)) {
					
					UserInfo.StartPlacing(arg.Player(), deployabletype);
				}
			}
		}



		#region Debug tools

		// Lists the ent's components and variables to player's chat

		void ListComponentsDebug(BasePlayer player, BaseEntity ent) {

			List<string> lines = new List<string>();
			string s = "<color=#80c5ff>───────────────────────</color>";
			int limit = 1030;

			foreach (var c in ent.GetComponents<Component>()) {

				List<string> types = new List<string>();
				List<string> names = new List<string>();
				List<string> values = new List<string>();
				int typelength = 0;

				foreach (FieldInfo fi in c.GetType().GetFields()) {

					System.Object obj = (System.Object) c;
					string ts = fi.FieldType.Name;
					if (ts.Length > typelength)
						typelength = ts.Length;

					types.Add(ts);
					names.Add(fi.Name);

					var val = fi.GetValue(obj);
					if (val != null)
						values.Add(val.ToString());
					else
						values.Add("null");

				}

				if (s.Length > 0)
					s += "\n";
				s += types.Count > 0 ? "╔" : "═";
				s += $" {c.GetType()} : {c.GetType().BaseType}";
				//s += " <" + c.name + ">\n";

				for (int i = 0; i < types.Count; i++) {

					string ns = $"<color=#80c5ff> {types[i]}</color> {names[i]} = <color=#00ff00>{values[i]}</color>";

					if (s.Length + ns.Length >= limit) {
						lines.Add(s);
						s = "║" + ns;
					} else {
						s += "\n║" + ns;
					}
				}

				if (types.Count > 0) {
					s += "\n╚══";
					lines.Add(s);
					s = string.Empty;
				}
			}

			lines.Add(s);

			foreach (string ls in lines)
				PrintToChat(player, ls);

		}

		#endregion
	}
}

namespace Oxide.Plugins.JCore {

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

		}
	}

}

namespace Oxide.Plugins.JCore {

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

namespace Oxide.Plugins.JCore {

	public interface IUpdateable {

		void Update(int TickDelta);
		
	}

}

namespace Oxide.Plugins.JCore {

	public class JDeployable {

		// TODO
		// handle entity hooks (health, damage, repair)
		// owner, parent
		// cui menu
		// spawn/destroy 
		// load/save

		public int Id;
		public ulong ownerId;
		public string ownerName;
		public bool isEnabled;
		public float health;

		private BaseEntity MainParent;
		private List<BaseEntity> ChildEntities = new List<BaseEntity>();

		public void SetMainParent(BaseEntity baseEntity) {
			// TODO attach a component here

			MainParent = baseEntity;
			MainParent.enableSaving = false;
		}

		public void AddChildEntity(BaseEntity baseEntity) {
			// TODO attach a component here

			baseEntity.SetParent(baseEntity);
			baseEntity.enableSaving = false;
			ChildEntities.Add(baseEntity);
		}
		
		
		#region Placing

		/// <summary>
		/// Can User start placing this deployable?
		/// Item requirements are already handled by UserInfo.
		/// This is if you want to add custom requirements to creating your deployable.
		/// </summary>
		/// <returns>if user can start placing this deployable</returns>
		public static bool CanStartPlacing(UserInfo userInfo) {
			return true;
		}

		/// <summary>
		/// Called when player starts placing this deployable.
		/// Use this for displaying instructions via userInfo.ShowMessage().
		/// To stop placing you can either call userInfo.DonePlacing() or userInfo.CancelPlacing().
		/// </summary>
		public static void OnStartPlacing(UserInfo userInfo) {

		}

		/// <summary>
		/// Called after userInfo.DonePlacing() or userInfo.CancelPlacing().
		/// Use this to clean up anything left over from placing your deployable (visual aids, placeholder items, etc).
		/// </summary>
		public static void OnEndPlacing(UserInfo userInfo) {

		}

		/// <summary>
		/// Called when player is placing this deployable and hits an entity with a hammer.
		/// Use this if you want the player to select existing entities.
		/// Use userInfo.placingdata to store selected entities or other data.
		/// </summary>
		public static void OnPlacingHammerHit(UserInfo userInfo, HitInfo hit) {

		}
		
		/// <summary>
		/// Spawn a deployable from placing command and set default values.
		/// </summary>
		/// <returns>If successfully placed</returns>
		public virtual bool Place(UserInfo userInfo) {
			// set deployable variables here
			return Spawn();
		}

		#endregion
		

		public virtual bool Spawn() {
			// spawn from variables
			return false;
		}


		public virtual void Update(int TickDelta) {

		}
		

	}

}

namespace Oxide.Plugins.JCore {

	public class JDeployableManager {

		// TODO
		// manage spawned deployables
		// distributive deployable update
		// load deployable types
		// load and spawn deployables from save file (async)
		// save deployables
		// clean up deployables on unload

		public static Dictionary<Type, JInfoAttribute> DeployableTypes = new Dictionary<Type, JInfoAttribute>();
		public static Dictionary<Type, List<JRequirementAttribute>> DeployableTypeRequirements = new Dictionary<Type, List<JRequirementAttribute>>();

		public static Dictionary<int, JDeployable> activeDeployables = new Dictionary<int, JDeployable>();

		/// <summary>
		/// JDeployable API
		/// Registers JDeployable to the JDeployableManager
		/// </summary>
		/// <typeparam name="T">JDeployable</typeparam>
		public static void RegisterJDeployable<T>() where T : JDeployable {

			// get info attribute
			JInfoAttribute info = (JInfoAttribute) System.Attribute.GetCustomAttribute(typeof(T), typeof(JInfoAttribute));

			if (info == null) {
				Interface.Oxide.LogWarning($"[JDeployableManager] Failed to register ({typeof(T)}) - Missing JInfoAttribute.");
				return;
			}

			if (DeployableTypes.ContainsKey(typeof(T)) || DeployableTypeRequirements.ContainsKey(typeof(T))) {
				Interface.Oxide.LogWarning($"[JDeployableManager] [{info.PluginInfo.Title}] {info.Name} has already been registered!");
				return;
			}

			// get requirements attributes
			List<JRequirementAttribute> requirements = System.Attribute.GetCustomAttributes(typeof(T), typeof(JRequirementAttribute)).OfType<JRequirementAttribute>().ToList();

			if (requirements == null || requirements.Count == 0) {
				Interface.Oxide.LogWarning($"[JDeployableManager] Failed to register ({typeof(T)}) - Missing JRequirementAttribute.");
				return;
			} else if (requirements.Count > 5) {
				Interface.Oxide.LogWarning($"[JDeployableManager] Failed to register ({typeof(T)}) - More than 5 JRequirementAttribute are not allowed.");
				return;
			}
			
			DeployableTypes.Add(typeof(T), info);
			DeployableTypeRequirements.Add(typeof(T), requirements);
			Interface.Oxide.LogInfo($"[JDeployableManager] Registered Deployable: [{info.PluginInfo.Title}] {info.Name}");
			
		}

		/// <summary>
		/// JDeployable API
		/// Unregisters JDeployable from the JDeployableManager
		/// </summary>
		/// <typeparam name="T">JDeployable</typeparam>
		public static void UnregisterJDeployable<T>() where T : JDeployable {

			// get info attribute
			JInfoAttribute info = (JInfoAttribute) System.Attribute.GetCustomAttribute(typeof(T), typeof(JInfoAttribute));

			if (DeployableTypes.Remove(typeof(T)) && DeployableTypeRequirements.Remove(typeof(T))) {
				Interface.Oxide.LogInfo($"[JCore] Unregistered Deployable: [{info.PluginInfo.Title}] {info.Name}");
			} else {
				Interface.Oxide.LogInfo($"[JCore] Failed to Unregistered Deployable: [{info.PluginInfo.Title}] {info.Name}");
			}
		}

		public static bool TryGetType(string name, out Type deployabletype) {

			foreach (Type type in DeployableTypes.Keys)
				if (type.FullName == name) {
					deployabletype = type;
					return true;
				}

			deployabletype = null;
			return false;
		}

		private static System.Random IDGenerator = new System.Random();
		private static int NewUID() {
			int id = (int) IDGenerator.Next(0, int.MaxValue);
			if (activeDeployables.ContainsKey(id))
				return NewUID();
			else
				return id;
		}

		public static bool PlaceDeployable(Type deployabletype, UserInfo userInfo) {

			var instance = Activator.CreateInstance(deployabletype);

			var methodInfo = deployabletype.GetMethod("Place");
			if (!(methodInfo != null && (bool) methodInfo.Invoke(instance, new object[] { userInfo })))
				return false;

			var fieldInfo = deployabletype.GetField("Id");
			if (fieldInfo == null)
				return false;

			int id = NewUID();
			fieldInfo.SetValue(instance, id);
			
			activeDeployables.Add(id, (JDeployable) instance);
			
			return true;
		}

	}
}

namespace Oxide.Plugins.JCore {

	public class UserInfo : MonoBehaviour {

		public BasePlayer player;
		public InputState input;

		private bool isHoldingHammer;
		private bool isDown;
		private uint lastActiveItem;
		private float startPressingTime;

		private string overlay; // uid for overlay cui instance
		private string messageoverlay;
		private string currentmessageoverlaytext;
		private string currentmessageoverlaysubtext;
		private bool isOverlayOpen;
		private Coroutine MessageTextShow;
		private Coroutine MessageTextHide;

		private bool isPlacing;
		private Type placingType;
		public List<BaseEntity> placingSelected;

		/// <summary>
		/// Get/create UserInfo from a BasePlayer.
		/// </summary>
		public static UserInfo Get(BasePlayer basePlayer) {
			return basePlayer.GetComponent<UserInfo>() ?? basePlayer.gameObject.AddComponent<UserInfo>();
		}

		void Awake() {
			player = GetComponent<BasePlayer>();
			input = player.serverInput;
			enabled = true;
			lastActiveItem = 0;
			isOverlayOpen = false;
		}


		/// <summary>
		/// MonoBehavior Update
		/// </summary>
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

		#region Hooks

		/// <summary>
		/// When player's held item is changed.
		/// </summary>
		private void OnPlayerActiveItemChanged() {
			var item = player.GetActiveItem();
			isHoldingHammer = (item != null && item.info != null && (item.info.name == "hammer.item"));

			// TODO if change from placeholder item, EndPlacing()
		}

		/// <summary>
		/// OnHammerHit for this player
		/// </summary>
		public static void OnHammerHit(BasePlayer basePlayer, HitInfo hit) => Get(basePlayer).OnHammerHit(hit);

		/// <summary>
		/// OnHammerHit for this player
		/// </summary>
		public void OnHammerHit(HitInfo hit) {
			if (isPlacing) {
				placingType.GetMethod("OnPlacingHammerHit")?.Invoke(null, new object[] { this, hit });
			}
		}

		#endregion

		#region Crafting

		/// <summary>
		/// Can player craft deployable
		/// </summary>
		/// <param name="jdeployabletype"></param>
		/// <returns></returns>
		public bool CanCraftDeployable(Type jdeployabletype) {

			List<JRequirementAttribute> requirements;
			JDeployableManager.DeployableTypeRequirements.TryGetValue(jdeployabletype, out requirements);

			if (requirements == null) return false;

			foreach (JRequirementAttribute req in requirements) {
				if (!this.DoesHaveUsableItem(req.ItemId, req.ItemAmount))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Player has item amount in their inventory
		/// </summary>
		/// <param name="item"></param>
		/// <param name="iAmount"></param>
		/// <returns></returns>
		public bool DoesHaveUsableItem(int item, int iAmount) {
			int num = 0;
			foreach (ItemContainer container in player.inventory.crafting.containers)
				num += container.GetAmount(item, true);
			return num >= iAmount;
		}
		
		/// <summary>
		/// Collect required ingredients for deployable
		/// </summary>
		/// <param name="jdeployabletype"></param>
		/// <returns></returns>
		private void CollectIngredients(Type jdeployabletype) {
			
			List<JRequirementAttribute> requirements;
			JDeployableManager.DeployableTypeRequirements.TryGetValue(jdeployabletype, out requirements);

			List<Item> collect = new List<Item>();
			
			foreach (JRequirementAttribute req in requirements) {
				this.CollectIngredient(req.ItemId, req.ItemAmount, collect);
				player.Command($"note.inv {req.ItemId} -{req.ItemAmount}");
			}

			foreach (Item obj in collect)
				obj.Remove(0.0f);
		}

		private void CollectIngredient(int item, int amount, List<Item> collect) {
			foreach (ItemContainer container in player.inventory.crafting.containers) {
				amount -= container.Take(collect, item, amount);
				if (amount <= 0)
					break;
			}
		}

		#endregion

		#region CUI

		/// <summary>
		/// Show overlay menu for the given BasePlayer
		/// </summary>
		public static void ShowOverlay(BasePlayer basePlayer) => Get(basePlayer).ShowOverlay();

		/// <summary>
		/// Show overlay menu for parent player
		/// </summary>
		public void ShowOverlay() {
			HideOverlay(); // just in case
			CancelPlacing(); // cancel placing
			
			var elements = new CuiElementContainer();

			overlay = Cui.Menu.CreateOverlay(elements, this);

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
		/// Destroy all userinfo cui for the player
		/// </summary>
		public void DestroyCui() {
			HideOverlay();
			HideMessageText();
		}

		/// <summary>
		/// Shows message text for player
		/// </summary>
		/// <param name="message">message text</param>
		/// <param name="submessage">subtext message text</param>
		/// <param name="duration">duration of the message</param>
		/// <param name="delay">delay before showing the message</param>
		public void ShowMessage(string message, string subtext = "", float duration = -1f, float delay = 0f) {

			if (MessageTextShow != null)
				StopCoroutine(MessageTextShow); // cancel previous delayed show

			if (delay > 0) {
				MessageTextShow = StartCoroutine(DelayShow(delay, message, subtext, duration));
			} else {
				ShowMessageText(message, subtext);
			}
			
			if (duration > 0)
				HideMessageText(duration);
		}

		private IEnumerator DelayShow(float delay, string message, string subtext, float duration) {
			yield return new WaitForSecondsRealtime(delay);

			ShowMessage(message, subtext, duration);

			MessageTextShow = null;
		}

		/// <summary>
		/// Shows error message text for player
		/// </summary>
		/// <param name="message">message text</param>
		/// <param name="submessage">subtext message text</param>
		public void ShowErrorMessage(string message, string subtext = "", float duration = 2f) {
			ShowMessageText(message, subtext, "1 0.5 0.2 1");
			if (duration > 0)
				HideMessageText(duration);
		}

		private void ShowMessageText(string text, string subtext = "", string textcolor = "1.0 1.0 1.0 1.0") {

			HideMessageText();

			var elements = new CuiElementContainer();

			messageoverlay = elements.Add(
				Cui.CreatePanel("0.3 0.3", "0.7 0.35", "0 0 0 0")
			);

			elements.Add(
				Cui.AddOutline(
				new CuiLabel {
					Text = { Text = (subtext != "") ? $"{text}\n<size=12>{subtext}</size>" : text, FontSize = 14, Align = TextAnchor.MiddleCenter, Color = textcolor },
					RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
					FadeOut = 2f
				},
				messageoverlay)
			);

			CuiHelper.AddUi(player, elements);

			currentmessageoverlaytext = text;
			currentmessageoverlaysubtext = subtext;
		}

		/// <summary>
		/// Hide current message text for player with optional delay
		/// </summary>
		public void HideMessageText(float delay = 0) {

			if (MessageTextHide != null)
				StopCoroutine(MessageTextHide); // cancel previous delayed hide

			if (delay > 0) {
				string oldoverlay = messageoverlay;
				string beforetext = currentmessageoverlaytext;
				string beforesub = currentmessageoverlaysubtext;
				MessageTextHide = StartCoroutine(DelayHide(delay, oldoverlay, beforetext, beforesub));
			} else {
				if (!string.IsNullOrEmpty(messageoverlay))
					CuiHelper.DestroyUi(player, messageoverlay);
				currentmessageoverlaytext = string.Empty;
				currentmessageoverlaysubtext = string.Empty;
			}
		}

		private IEnumerator DelayHide(float delay, string oldoverlay, string beforetext, string beforesub) {
			yield return new WaitForSecondsRealtime(delay);

			if (!string.IsNullOrEmpty(messageoverlay))
				CuiHelper.DestroyUi(player, messageoverlay);
			if (beforetext == currentmessageoverlaytext)
				currentmessageoverlaytext = string.Empty;
			if (beforesub == currentmessageoverlaysubtext)
				currentmessageoverlaysubtext = string.Empty;

			MessageTextHide = null;
		}

		#endregion

		#region Deployable Placing

		/// <summary>
		/// Start placing deployable
		/// </summary>
		public static void StartPlacing(BasePlayer basePlayer, Type deployabletype) => Get(basePlayer).StartPlacing(deployabletype);

		/// <summary>
		/// Start placing deployable
		/// </summary>
		public void StartPlacing(Type deployabletype) {

			// ask deployable type if we can start placing it
			var methodInfo = deployabletype.GetMethod("CanStartPlacing");
			if (methodInfo != null) {
				if (!(bool) methodInfo.Invoke(null, new object[] { this }))
					return;
			}

			HideOverlay();
			
			isPlacing = true;
			placingType = deployabletype;
			placingSelected = new List<BaseEntity>();

			var startplacingmethod = deployabletype.GetMethod("OnStartPlacing");
			if (startplacingmethod != null)
				startplacingmethod.Invoke(null, new object[] { this });
			
		}

		private void EndPlacing() {

			if (!isPlacing)
				return;

			if (placingType != null) {
				var methodInfo = placingType.GetMethod("OnEndPlacing");
				if (methodInfo != null)
					methodInfo.Invoke(null, new object[] { this });
			}

			isPlacing = false;
			placingType = null;
			placingSelected.Clear();

			HideMessageText();
		}

		/// <summary>
		/// Cancel placing deployable
		/// </summary>
		public void CancelPlacing() {
			EndPlacing();
		}

		/// <summary>
		/// Done placing deployable
		/// </summary>
		public void DonePlacing() {
			
			if (!isPlacing)
				return;
			
			if (CanCraftDeployable(placingType) && JDeployableManager.PlaceDeployable(placingType, this)) { // if player can craft it and it is placed

				CollectIngredients(placingType); // consume ingredients from player's inventory
			}
			
			EndPlacing();
		}

		#endregion

	}

}

namespace Oxide.Plugins.JCore {

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

namespace Oxide.Plugins.JCore {

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class JRequirementAttribute : Attribute {

		public ItemDefinition itemDef { get; }
		public string ItemShortName { get; }
		public int ItemId { get; }
		public int ItemAmount { get; }
		public string PerUnit { get; }

		/// <summary>
		/// Required Item and amount for placing deployable
		/// </summary>
		/// <param name="itemShortName">Shortname of item definition.  Check out the Oxide docs for a list of shortnames.</param>
		/// <param name="itemAmount">Amount required for the item</param>
		/// <param name="perUnit">Unit per amount required (ex. Transport Pipe is itemAmount per "segment")</param>
		public JRequirementAttribute(string itemShortName, int itemAmount = 1, string perUnit = null) {
			this.ItemShortName = itemShortName;
			this.ItemAmount = itemAmount;
			this.PerUnit = perUnit ?? string.Empty;

			this.itemDef = ItemManager.FindItemDefinition(this.ItemShortName);
			this.ItemId = this.itemDef.itemid;
		}

		/// <summary>
		/// Required Item and amount for placing deployable
		/// </summary>
		/// <param name="itemId">itemId of item definition.  Check out the Oxide docs for a list of itemIds.</param>
		/// <param name="itemAmount">Amount required for the item</param>
		/// <param name="perUnit">Unit per amount required (ex. Transport Pipe is itemAmount per "segment")</param>
		public JRequirementAttribute(int itemId, int itemAmount = 1, string perUnit = null) {
			this.ItemId = itemId;
			this.ItemAmount = itemAmount;
			this.PerUnit = perUnit ?? string.Empty;

			this.itemDef = ItemManager.FindItemDefinition(this.ItemId);
			this.ItemShortName = this.itemDef.shortname;
		}
	}
}

namespace Oxide.Plugins.JCore.Util {

	public static class Icons {

		// list of items with shortnames: https://github.com/OxideMod/Oxide.Docs/blob/master/source/includes/rust/item_list.md
		// item icons: http://rust.wikia.com/wiki/Items

		private readonly static Dictionary<string, string> ItemUrls = new Dictionary<string, string>() {
			{ "autoturret", "http://vignette2.wikia.nocookie.net/play-rust/images/f/f9/Auto_Turret_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "bbq", "https://vignette.wikia.nocookie.net/play-rust/images/f/f8/Barbeque_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "box.repair.bench", "http://vignette1.wikia.nocookie.net/play-rust/images/3/3b/Repair_Bench_icon.png/revision/latest/scale-to-width-down/{0}" },
			
			{ "gears", "https://vignette.wikia.nocookie.net/play-rust/images/7/72/Gears_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "metal.fragments", "https://vignette.wikia.nocookie.net/play-rust/images/7/74/Metal_Fragments_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "metal.refined", "https://vignette.wikia.nocookie.net/play-rust/images/a/a1/High_Quality_Metal_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "scrap", "https://vignette.wikia.nocookie.net/play-rust/images/0/03/Scrap_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "vending.machine", "http://vignette2.wikia.nocookie.net/play-rust/images/5/5c/Vending_Machine_icon.png/revision/latest/scale-to-width-down/{0}" },

			{ "wood", "https://vignette.wikia.nocookie.net/play-rust/images/f/f2/Wood_icon.png/revision/latest/scale-to-width-down/{0}" },

			// TODO
			// convert to shortnames
			// sort alphabetically

			{ "Small_Stocking", "http://vignette2.wikia.nocookie.net/play-rust/images/9/97/Small_Stocking_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "SUPER_Stocking", "http://vignette1.wikia.nocookie.net/play-rust/images/6/6a/SUPER_Stocking_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Small_Present", "http://vignette2.wikia.nocookie.net/play-rust/images/d/da/Small_Present_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Medium_Present", "http://vignette3.wikia.nocookie.net/play-rust/images/6/6b/Medium_Present_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Large_Present", "http://vignette1.wikia.nocookie.net/play-rust/images/9/99/Large_Present_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Pump_Jack", "http://vignette2.wikia.nocookie.net/play-rust/images/c/c9/Pump_Jack_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Shop_Front", "http://vignette4.wikia.nocookie.net/play-rust/images/c/c1/Shop_Front_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Water_Purifier", "http://vignette3.wikia.nocookie.net/play-rust/images/6/6e/Water_Purifier_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Water_Barrel", "http://vignette4.wikia.nocookie.net/play-rust/images/e/e2/Water_Barrel_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Survival_Fish_Trap", "http://vignette2.wikia.nocookie.net/play-rust/images/9/9d/Survival_Fish_Trap_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Research_Table", "http://vignette2.wikia.nocookie.net/play-rust/images/2/21/Research_Table_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Small_Planter_Box", "http://vignette3.wikia.nocookie.net/play-rust/images/a/a7/Small_Planter_Box_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Large_Planter_Box", "http://vignette1.wikia.nocookie.net/play-rust/images/3/35/Large_Planter_Box_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Jack_O_Lantern_Happy", "http://vignette1.wikia.nocookie.net/play-rust/images/9/92/Jack_O_Lantern_Happy_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Jack_O_Lantern_Angry", "http://vignette4.wikia.nocookie.net/play-rust/images/9/96/Jack_O_Lantern_Angry_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Large_Furnace", "http://vignette3.wikia.nocookie.net/play-rust/images/e/ee/Large_Furnace_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Ceiling_Light", "http://vignette3.wikia.nocookie.net/play-rust/images/4/43/Ceiling_Light_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Hammer", "http://vignette4.wikia.nocookie.net/play-rust/images/5/57/Hammer_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Camp_Fire", "http://vignette4.wikia.nocookie.net/play-rust/images/3/35/Camp_Fire_icon.png/revision/latest/scale-to-width-down/{0}" },	
			{ "Furnace", "http://vignette4.wikia.nocookie.net/play-rust/images/e/e3/Furnace_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Lantern", "http://vignette4.wikia.nocookie.net/play-rust/images/4/46/Lantern_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Large_Water_Catcher", "http://vignette2.wikia.nocookie.net/play-rust/images/3/35/Large_Water_Catcher_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Large_Wood_Box", "http://vignette1.wikia.nocookie.net/play-rust/images/b/b2/Large_Wood_Box_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Mining_Quarry", "http://vignette1.wikia.nocookie.net/play-rust/images/b/b8/Mining_Quarry_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Small_Oil_Refinery", "http://vignette2.wikia.nocookie.net/play-rust/images/a/ac/Small_Oil_Refinery_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Small_Stash", "http://vignette2.wikia.nocookie.net/play-rust/images/5/53/Small_Stash_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Small_Water_Catcher", "http://vignette2.wikia.nocookie.net/play-rust/images/0/04/Small_Water_Catcher_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Search_Light", "http://vignette2.wikia.nocookie.net/play-rust/images/c/c6/Search_Light_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Wood_Storage_Box", "http://vignette2.wikia.nocookie.net/play-rust/images/f/ff/Wood_Storage_Box_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Drop_Box", "http://vignette2.wikia.nocookie.net/play-rust/images/4/46/Drop_Box_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Fridge", "http://vignette2.wikia.nocookie.net/play-rust/images/8/88/Fridge_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Shotgun_Trap", "http://vignette2.wikia.nocookie.net/play-rust/images/6/6c/Shotgun_Trap_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Flame_Turret", "http://vignette2.wikia.nocookie.net/play-rust/images/f/f9/Flame_Turret_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Recycler", "http://vignette2.wikia.nocookie.net/play-rust/images/e/ef/Recycler_icon.png/revision/latest/scale-to-width-down/{0}" },
			{ "Tool_Cupboard", "http://vignette2.wikia.nocookie.net/play-rust/images/5/57/Tool_Cupboard_icon.png/revision/latest/scale-to-width-down/{0}" }
		};


		public static string GetItemIconURL(string name, int size) {
			string url;
			if (ItemUrls.TryGetValue(name, out url)) {
				return string.Format(url, size);
			}
			return string.Empty;
		}


	}

}

namespace Oxide.Plugins.JTechDeployables {

	[JInfo(typeof(JTech), "Assembler", "https://i.imgur.com/R9mD3VQ.png")]
	[JRequirement("vending.machine"), JRequirement("gears", 5), JRequirement("metal.refined", 20)]

	public class Assembler : JDeployable {



	}
}

namespace Oxide.Plugins.JTechDeployables {

	[JInfo(typeof(JTech), "Auto Farm", "https://i.imgur.com/lEXshkx.png")]
	[JRequirement("scrap", 10)]

	public class AutoFarm : JDeployable {



	}
}

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

			return true;
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

namespace Oxide.Plugins.JTechDeployables {

	[JInfo(typeof(JTech), "Trash Can", "https://i.imgur.com/lEXshkx.png")]
	[JRequirement("scrap", 10)]

	public class TrashCan : JDeployable {



	}
}
