using System;
using Oxide.Core;
using Oxide.Plugins;
using System.Collections.Generic;

namespace Oxide.Plugins.JCore {

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class JRequirementAttribute : Attribute {
		
		public string ItemShortName { get; }
		public int ItemAmount { get; }
		public string PerUnit { get; }

		/// <summary>
		/// Required Item and amount for placing deployable
		/// </summary>
		/// <param name="itemShortName">Shortname of item definition.  Check out the Oxide docs for a list of shortnames.</param>
		/// <param name="itemAmount">Amount required for the item</param>
		/// <param name="perUnit">Unit per amount required (ex. Transport Pipe is itemAmount per "segment")</param>
		public JRequirementAttribute(string itemShortName, int itemAmount = 1, string perUnit = null) {
			this.ItemShortName = itemShortName;
			this.ItemAmount = itemAmount;
			this.PerUnit = perUnit ?? string.Empty;
		}
	}
}