using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Game.Rust.Cui;

namespace Oxide.Plugins.JCore {

	public class UserInfo : MonoBehaviour {

		public BasePlayer player;
		public InputState input;

		private bool isHoldingHammer;
		private bool isHoldingPlaceholder;
		private bool isDown;
		private Item lastActiveItem;
		private float startPressingTime;

		private string overlay; // uid for overlay cui instance
		private string messageoverlay;
		private string currentmessageoverlaytext;
		private string currentmessageoverlaysubtext;
		private bool isOverlayOpen;
		private Coroutine MessageTextShow;
		private Coroutine MessageTextHide;

		private Coroutine _HidePlaceholder;
		private bool isPlacing;
		private Type placingType;
		public List<BaseEntity> placingSelected;
		private Item placingPlaceholder;
		private BaseEntity placingPlaceholderPlaced;
		private bool placingtookhammer = false;
		private bool movingplaceholder = false;
		private ulong hammerskin;
		private int beltposition;

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
			lastActiveItem = null;
			isOverlayOpen = false;
			beltposition = -1;
		}


		/// <summary>
		/// MonoBehavior Update
		/// </summary>
		void Update() {

			// TODO detect when on a pipe and set violationlevel to 0
			//player.violationLevel = 0;
			
			if (player.GetActiveItem() != lastActiveItem) {
				lastActiveItem = player.GetActiveItem();
				OnPlayerActiveItemChanged(lastActiveItem);
			}

			if (!isOverlayOpen && (isHoldingHammer || isPlacing)) {
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

		/// <summary>
		/// Monobehavior OnDestroy
		/// </summary>
		void OnDestroy() {
			DestroyCui();
		}

		#region Hooks

		/// <summary>
		/// When player's held item is changed.
		/// </summary>
		private void OnPlayerActiveItemChanged(Item item) {
			
			isHoldingHammer = item != null && item.info != null && (item.info.name == "hammer.item");
			
			// if player changes held item while placing
			if (item != null && beltposition != -1 && item.position != beltposition) {
				CancelPlacing();
				HideOverlay();
			}
		}

		public bool? CanMoveItem(Item item, int targetSlot) {
			if (isPlacing && beltposition != -1 && (item.position == beltposition || targetSlot == beltposition)) {
				CancelPlacing();
				return false;
			}
			if (isOverlayOpen && item.info != null && item.info.name == "hammer.item") {
				CancelPlacing();
				HideOverlay();
				return false;
			}
			return null;
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
				placingType?.GetMethod("OnPlacingHammerHit")?.Invoke(null, new object[] { this, hit });
			}
		}

		/// <summary>
		/// OnDeployPlaceholder for this player
		/// </summary>
		public static void OnDeployPlaceholder(BasePlayer basePlayer, BaseEntity entity) => Get(basePlayer).OnDeployPlaceholder(entity);

		/// <summary>
		/// OnDeployPlaceholder for this player
		/// </summary>
		public void OnDeployPlaceholder(BaseEntity entity) {
			if (isPlacing && placingPlaceholder != null) {
				placingType?.GetMethod("OnDeployPlaceholder")?.Invoke(null, new object[] { this, entity });
				placingPlaceholder = null;
				placingPlaceholderPlaced = entity;
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
			beltposition = player.GetActiveItem().position;
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
			placingPlaceholder = (Item) deployabletype.GetMethod("GetPlaceholderItem")?.Invoke(null, new object[] { this });
			beltposition = player.GetActiveItem().position;

			if (placingPlaceholder != null) {
				ShowPlaceholder();
			}

			deployabletype.GetMethod("OnStartPlacing")?.Invoke(null, new object[] { this });
			
		}

		private void ShowPlaceholder() {
			if (!isPlacing || placingPlaceholder == null || !isHoldingHammer)
				return;

			if (_HidePlaceholder != null)
				StopCoroutine(_HidePlaceholder); // cancel previous

			hammerskin = player.GetActiveItem().skin;
			placingtookhammer = true;
			movingplaceholder = true;
			player.GetActiveItem().Remove();
			placingPlaceholder.MoveToContainer(player.inventory.containerBelt, beltposition);
			movingplaceholder = false;
		}

		private void HidePlaceholder() {

			if (_HidePlaceholder != null)
				StopCoroutine(_HidePlaceholder); // cancel previous
				
			_HidePlaceholder = StartCoroutine(DelayHidePlaceholder());
		}

		private IEnumerator DelayHidePlaceholder() {
			yield return new WaitForSecondsRealtime(0.01f);

			if (placingPlaceholderPlaced != null) {
				placingPlaceholderPlaced.Kill();
			}

			if (placingtookhammer)
				ItemManager.CreateByName("hammer", 1, hammerskin).MoveToContainer(player.inventory.containerBelt, beltposition);
			placingtookhammer = false;
			beltposition = -1;
			placingPlaceholder = null;
			placingPlaceholderPlaced = null;

			_HidePlaceholder = null;
		}

		private void EndPlacing() {

			if (!isPlacing)
				return;
			
			placingType?.GetMethod("OnEndPlacing")?.Invoke(null, new object[] { this });
			
			isPlacing = false;
			placingType = null;
			placingSelected.Clear();

			HidePlaceholder();
		}

		/// <summary>
		/// Cancel placing deployable
		/// </summary>
		public void CancelPlacing() {

			if (!isPlacing)
				return;

			ShowMessage($"Canceled Placing", "", 1);

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

				JInfoAttribute info;
				JDeployableManager.DeployableTypes.TryGetValue(placingType, out info);

				ShowMessage($"{info.Name} Created", "", 3);
			}

			EndPlacing();
		}

		#endregion

	}

}