﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins.JCore {

	public class JDeployable {

		public List<ItemAmount> ingredients = new List<ItemAmount>(){};

		// TODO
		// handle entity hooks (health, damage, repair)
		// owner, parent
		// cui menu
		// spawn/destroy 
		// load/save


		public JDeployable() { }
	}

}