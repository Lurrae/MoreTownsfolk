using MoreTownPets.Items;

namespace MoreTownPets.NPCs
{
	// Edit town NPC shops
	public class ShopGlobalNPC : GlobalNPC
	{
		public override void ModifyShop(NPCShop shop)
		{
			// Zoologist shop
			if (shop.NpcType == NPCID.BestiaryGirl)
			{
				shop.InsertAfter(ItemID.LicenseBunny, ItemType<LicenseAxolotl>(), ExtraConditions.BestiaryCompletionPercent(0.65f));
			}
		}
	}
}