using Terraria.GameContent;

namespace MoreTownsfolk.Gores
{
	public class ShinobiDecoy : ModGore
	{
		public override void SetStaticDefaults()
		{
			// Allows the decoy to spawn when "Blood and Gore" is disabled, since it's not really "gore" in the traditional sense
			ChildSafety.SafeGore[Type] = true;
		}
	}
}