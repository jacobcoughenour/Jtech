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
		private bool isDown;
		private uint lastActiveItem;
		private float startPressingTime;

		private string overlay; // uid for overlay cui instance
		private string messageoverlay;
		private string currentmessageoverlaytext;
		private string currentmessageoverlaysubtext;
		private bool isOverlayOpen;
		private Coroutine MessageTextHide;

		private bool isPlacing;
		private Type placingType;

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

		public bool DoesHaveUsableItem(int item, int iAmount) {
			int num = 0;
			foreach (ItemContainer container in player.inventory.crafting.containers)
				num += container.GetAmount(item, true);
			return num >= iAmount;
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
		/// Shows error message text for player
		/// </summary>
		/// <param name="message">message text</param>
		/// <param name="submessage">subtext message text</param>
		public void ShowErrorMessage(string message, string subtext = "") {
			ShowMessageText(message, subtext);
			HideMessageText(2f);
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

		/// <summary>
		/// Start placing a deployable
		/// </summary>
		public static void StartPlacing(BasePlayer basePlayer, Type deployabletype) => Get(basePlayer).StartPlacing(deployabletype);

		/// <summary>
		/// Start placing deployable
		/// </summary>
		public void StartPlacing(Type deployabletype) {
		

			var methodInfo = deployabletype.GetMethod("CanStartPlacing");
			if (methodInfo != null) {
				if (!(bool) methodInfo.Invoke(null, new object[] { this }))
					return;
			}

			// start placing

			HideOverlay();
			
			isPlacing = true;
			placingType = deployabletype;


		}

		/// <summary>
		/// Stop placing deployable
		/// </summary>
		public void StopPlacing() {
			
			var methodInfo = placingType.GetMethod("StopPlacing");
			if (methodInfo != null)
				methodInfo.Invoke(null, new object[] { this });
			
			isPlacing = false;
			placingType = null;

		}



	}

}