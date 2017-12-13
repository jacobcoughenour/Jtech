using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oxide.Plugins.JTechCore;

namespace Oxide.Plugins.JTechCore {

	public class DeployableManager {
		
		private Dictionary<ulong, BaseDeployable> registeredDeployables = new Dictionary<ulong, BaseDeployable>();

		// TODO
		// manage spawned deployables
		// distributive deployable update
		// load deployable types
		// load and spawn deployables from save file (async)
		// save deployables
		// clean up deployables on unload




	}
}
