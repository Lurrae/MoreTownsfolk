using MoreTownsfolk.Configs;
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

			// Remove the world evil pylons from all town NPC shops other than the evil fanatics
			if (shop.NpcType != NPCType<Occultist>() && shop.ActiveEntries.Any(e => e.Item.type == ItemType<TeleportationPylonCorruption>()))
			{
				shop.GetEntry(ItemType<TeleportationPylonCorruption>()).Disable();
			}

			if (shop.NpcType != NPCType<Harvester>() && shop.ActiveEntries.Any(e => e.Item.type == ItemType<TeleportationPylonCrimson>()))
			{
				shop.GetEntry(ItemType<TeleportationPylonCrimson>()).Disable();
			}

			// Only apply the below changes if the config option is enabled; if it's disabled, leave vanilla pylon sales unchanged
			if (!GetInstance<ServerConfig>().ShuffleBiomePreferences)
				return;

			if (shop.ActiveEntries.Any(e => e.Item.type == ItemType<TeleportationPylonSky>() && !e.Conditions.Any(c => c == Condition.InSkyHeight)))
			{
				shop.GetEntry(ItemType<TeleportationPylonSky>()).AddCondition(Condition.InSkyHeight);
			}

			if (shop.ActiveEntries.Any(e => e.Item.type == ItemID.TeleportationPylonPurity && !e.Conditions.Any(c => c == ExtraConditions_MoreTownsfolk.NotInSkyHeight)))
			{
				shop.GetEntry(ItemID.TeleportationPylonPurity).AddCondition(ExtraConditions_MoreTownsfolk.NotInSkyHeight);
			}

			if (shop.ActiveEntries.Any(e => e.Item.type == ItemType<TeleportationPylonHell>() && !e.Conditions.Any(c => c == Condition.InUnderworldHeight)))
			{
				shop.GetEntry(ItemType<TeleportationPylonHell>()).AddCondition(Condition.InUnderworldHeight);
			}

			if (shop.ActiveEntries.Any(e => e.Item.type == ItemID.TeleportationPylonUnderground && !e.Conditions.Any(c => c == Condition.NotInUnderworld)))
			{
				shop.GetEntry(ItemID.TeleportationPylonUnderground).AddCondition(Condition.NotInUnderworld);
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
			Dictionary<int, NPCHappiness> happiness = new();

			foreach (NPC npc in ContentSamples.NpcsByNetId.Values.Where(n => n.townNPC))
			{
				happiness.Add(npc.type, NPCHappiness.Get(npc.type));
			}

			// Harvester: Disliked by the Guide and Zoologist, hated by the Dryad
			happiness[NPCID.Guide].SetNPCAffection(harvesterType, AffectionLevel.Dislike);
			happiness[NPCID.BestiaryGirl].SetNPCAffection(harvesterType, AffectionLevel.Dislike);
			happiness[NPCID.Dryad].SetNPCAffection(harvesterType, AffectionLevel.Hate);

			// Occultist: Disliked by the Guide and Clothier, hated by the Dryad
			happiness[NPCID.Guide].SetNPCAffection(occultistType, AffectionLevel.Dislike);
			happiness[NPCID.Clothier].SetNPCAffection(occultistType, AffectionLevel.Dislike);
			happiness[NPCID.Dryad].SetNPCAffection(occultistType, AffectionLevel.Hate);

			// Only apply the below changes if the config option is enabled; if it's disabled, skip doing all of this
			if (!GetInstance<ServerConfig>().ShuffleBiomePreferences)
				return;

			// Nurse: Changed to like the Sky instead of the Hallow
			happiness[NPCID.Nurse].SetBiomeAffection<CustomShoppingBiomes.SkyBiome>(AffectionLevel.Like);
			happiness[NPCID.Nurse].SetBiomeAffection<HallowBiome>(0);

			// Merchant: Changed to like the Sky and dislike the Underworld instead of liking the Forest and disliking the Desert
			happiness[NPCID.Merchant].SetBiomeAffection<CustomShoppingBiomes.SkyBiome>(AffectionLevel.Like);
			happiness[NPCID.Merchant].SetBiomeAffection<CustomShoppingBiomes.HellBiome>(AffectionLevel.Dislike);
			happiness[NPCID.Merchant].SetBiomeAffection<ForestBiome>(0);
			happiness[NPCID.Merchant].SetBiomeAffection<DesertBiome>(0);

			// Tavernkeep: Changed to dislike the Sky instead of the Snow
			happiness[NPCID.DD2Bartender].SetBiomeAffection<CustomShoppingBiomes.SkyBiome>(AffectionLevel.Dislike);
			happiness[NPCID.DD2Bartender].SetBiomeAffection<SnowBiome>(0);

			// Mechanic: Dislikes the Underworld as well as Underground (since they are now separate)
			happiness[NPCID.Mechanic].SetBiomeAffection<CustomShoppingBiomes.HellBiome>(AffectionLevel.Dislike);

			// Angler: Dislikes the Underworld instead of the Desert
			happiness[NPCID.Angler].SetBiomeAffection<CustomShoppingBiomes.HellBiome>(AffectionLevel.Dislike);
			happiness[NPCID.Angler].SetBiomeAffection<DesertBiome>(0);

			// Thorium mod support
			if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
			{
				if (thorium.TryFind("WeaponMaster", out ModNPC weaponMaster))
				{
					// Weapons Master: Likes the Underworld as well as the Underground
					NPCHappiness.Get(weaponMaster.Type).SetBiomeAffection<CustomShoppingBiomes.HellBiome>(AffectionLevel.Like);
				}

				if (thorium.TryFind("Spiritualist", out ModNPC spiritualist))
				{
					// Spiritualist: Hates the Underworld, in addition to disliking the Underground
					NPCHappiness.Get(spiritualist.Type).SetBiomeAffection<CustomShoppingBiomes.HellBiome>(AffectionLevel.Hate);
				}
			}
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

	// Handles custom downed booleans for vanilla NPCs, like Eater of Worlds and Brain of Cthulhu
	public class DownedGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (npc.type == NPCID.BrainofCthulhu)
			{
				TownsfolkWorld.downedBrain = true;
			}

			if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail && npc.boss)
			{
				TownsfolkWorld.downedEater = true;
			}
		}
	}
}