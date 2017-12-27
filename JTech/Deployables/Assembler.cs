using Oxide.Plugins.JCore;

namespace Oxide.Plugins.JTechDeployables {

	[JInfo(typeof(JTech), "Assembler", "https://i.imgur.com/R9mD3VQ.png")]
	[JRequirement("vending.machine"), JRequirement("gears", 5), JRequirement("metal.refined", 20)]
	[JUpdate(10, 5)]

	public class Assembler : JDeployable {



	}
}