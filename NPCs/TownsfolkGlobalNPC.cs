using MoreTownsfolk.Items;
using Terraria.GameContent.Personalities;

namespace MoreTownsfolk.NPCs
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

	// Modify happiness data for vanilla NPCs
	public class HappinessGlobalNPC : GlobalNPC
	{
		// Happiness modifications
		public override void SetStaticDefaults()
		{
			int harvesterType = NPCType<Harvester>();
			int occultistType = NPCType<Occultist>();
			var guideHappiness = NPCHappiness.Get(NPCID.Guide);
			var dryadHappiness = NPCHappiness.Get(NPCID.Dryad);
			var clothierHappiness = NPCHappiness.Get(NPCID.Clothier);
			var zoologistHappiness = NPCHappiness.Get(NPCID.BestiaryGirl);

			// Harvester: Disliked by the Guide and Zoologist, hated by the Dryad
			guideHappiness.SetNPCAffection(harvesterType, AffectionLevel.Dislike);
			zoologistHappiness.SetNPCAffection(harvesterType, AffectionLevel.Dislike);
			dryadHappiness.SetNPCAffection(harvesterType, AffectionLevel.Hate);
			
			// Occultist: Disliked by the Guide and Clothier, hated by the Dryad
			guideHappiness.SetNPCAffection(occultistType, AffectionLevel.Dislike);
			clothierHappiness.SetNPCAffection(occultistType, AffectionLevel.Dislike);
			dryadHappiness.SetNPCAffection(occultistType, AffectionLevel.Hate);
		}

		// Custom dialogue can go here as needed
		// The code here is just an example since no NPCs need custom dialogue yet
		/*public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (npc.type == NPCID.Dryad)
			{
				foreach (NPC harvester in Main.npc.Where(n => n.active && n.type == NPCType<Harvester>()))
				{
					string dia_old = Language.GetTextValueWith("TownNPCMood_Dryad.HateNPC", harvester.FullName);
					string dia_new = Language.GetTextValueWith("Mods.MoreTownsfolk.NPCs.Dryad.TownNPCMood.HateNPC_Fanatics", harvester.FullName);

					Main.npcChatText = Main.npcChatText.Replace(dia_old, dia_new);
				}

				foreach (NPC occultist in Main.npc.Where(n => n.active && n.type == NPCType<Occultist>()))
				{
					string dia_old = Language.GetTextValueWith("TownNPCMood_Dryad.HateNPC", occultist.FullName);
					string dia_new = Language.GetTextValueWith("Mods.MoreTownsfolk.NPCs.Dryad.TownNPCMood.HateNPC_Fanatics", occultist.FullName);

					Main.npcChatText = Main.npcChatText.Replace(dia_old, dia_new);
				}
			}
		}*/
	}
}