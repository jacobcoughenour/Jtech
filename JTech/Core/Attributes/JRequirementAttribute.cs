using System;
using Oxide.Core;
using Oxide.Plugins;
using System.Collections.Generic;

namespace Oxide.Plugins.JCore {

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class JRequirementAttribute : Attribute {
		
		public string ItemShortName { get; }
		public int ItemAmount { get; }
		
		/// <summary>
		/// Required Item and amount for placing deployable
		/// </summary>
		/// <param name="itemshortname">Shortname of item definition.  Check out the Oxide docs for a list of shortnames.</param>
		/// <param name="itemamount">Amount required for the item</param>
		public JRequirementAttribute(string itemshortname, int itemamount = 1) {
			this.ItemShortName = itemshortname;
			this.ItemAmount = itemamount;
		}
	}
}