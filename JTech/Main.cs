using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.JCore;

namespace Oxide.Plugins {

	[Info("JTech", "TheGreatJ", "1.0.0")]
	class JTech : RustPlugin {
		

		void OnServerInitialized() {

			JDeployableManager.RegisterJDeployable<JTechDeployables.TransportPipe>();
			JDeployableManager.RegisterJDeployable<JTechDeployables.Assembler>();
		}
		


	}
}

namespace Oxide.Plugins.JTechDeployables {

	[JInfo(typeof(JTech), "Transport Pipe", "https://i.imgur.com/R9mD3VQ.png")]
	public class TransportPipe : JDeployable {



	}

	[JInfo(typeof(JTech), "Assembler", "https://i.imgur.com/R9mD3VQ.png")]
	public class Assembler : JDeployable {



	}
}