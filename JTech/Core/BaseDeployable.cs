using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins.JTechCore {

	public class BaseDeployable {

		// TODO
		// handle entity hooks (health, damage, repair)
		// owner, parent
		// cui menu
		// spawn/destroy
		// load/save


		public BaseDeployable() { }

		public bool Spawn(out string error) {
			error = string.Empty;


			return true;
		}

	}

	// component added to BaseEntity(s) for handling hooks
	public class BaseDeployableChild : MonoBehaviour {
		// TODO
	}

	// component added to parent for handling hooks and update
	public class BaseDeployableBehaviour : BaseDeployableChild {
		// TODO
	}

}