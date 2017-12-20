using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Libraries.Covalence;
using Oxide.Plugins.JCore;

namespace Oxide.Plugins.JTechDeployables {

	[JInfo(typeof(JTech), "Transport Pipe", "https://vignette.wikia.nocookie.net/play-rust/images/4/4a/Metal_Pipe_icon.png/revision/latest/scale-to-width-down/200")]
	[JRequirement("wood", 20, "segment")]
	public class TransportPipe : JDeployable {

		public static bool CanStartPlacing(UserInfo userInfo) {
			userInfo.ShowErrorMessage("error message");
			return true;
		}

	}
}