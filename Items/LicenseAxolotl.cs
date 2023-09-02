using MoreTownsfolk.NPCs;

namespace MoreTownsfolk.Items
{
	public class LicenseAxolotl : ModItem
	{
		public override void SetStaticDefaults()
		{
			// See localization files for name and tooltip

			Item.ResearchUnlockCount = 5;
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.LicenseCat);
		}

		public override bool? UseItem(Player player)
		{
			player.LicenseOrExchangePet(Item, ref TownsfolkWorld.boughtAxolotl, NPCType<Axolotl>(), "Mods.MoreTownsfolk.Common.AxoLicenseUsed", -14);

			return base.UseItem(player);
		}
	}
}