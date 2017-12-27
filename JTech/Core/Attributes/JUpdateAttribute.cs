using System;
using Oxide.Core;
using Oxide.Plugins;
using System.Collections.Generic;

namespace Oxide.Plugins.JCore {

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class JUpdateAttribute : Attribute {

		public int updateDelay;
		public int maxConcurrentUpdates;

		/// <summary>
		/// Update rate for JDeployable
		/// </summary>
		/// <param name="updateDelay">delay before calling update again</param>
		/// <param name="maxConcurrentUpdates">Max number of updates called at the same time for this deployable.  When exceeded, updateDelay is doubled</param>
		public JUpdateAttribute(int updateDelay, int maxConcurrentUpdates = 10) {
			this.updateDelay = updateDelay;
			this.maxConcurrentUpdates = maxConcurrentUpdates;
		}
	}
}