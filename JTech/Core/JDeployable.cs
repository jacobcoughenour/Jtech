using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins.JCore {

	public class JDeployable {

		// TODO
		// handle entity hooks (health, damage, repair)
		// owner, parent
		// cui menu
		// spawn/destroy 
		// load/save

		/// <summary>
		/// Can User start placing this deployable?
		/// Item requirements are already handled by UserInfo.
		/// This is if you want to add custom requirements to creating your deployable.
		/// </summary>
		/// <returns>if user can start placing this deployable</returns>
		public static bool CanStartPlacing(UserInfo userInfo) {
			return true;
		}

	}

}