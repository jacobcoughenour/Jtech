using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins.JCore {

	public interface IUpdateable {

		void Update(int TickDelta);
		
	}

}