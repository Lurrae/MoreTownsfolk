using MoreTownsfolk.Gores;
using TepigCore.Base.ModdedNPC;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;

namespace MoreTownsfolk.NPCs
{
	[AutoloadHead]
	public class Ninja : ModTownee
	{
		private static int ShimmerHeadIdx;
		private static Profiles.StackedNPCProfile Profile;

		public override string DialogueKey => "Mods.MoreTownsfolk.NPCs.Ninja.";
		public override bool IsMale => true;

		public override void TowneeStaticDefaults()
		{
			Main.npcFrameCount[Type] = 25; // Same frame count as the Merchant

			NPCID.Sets.ExtraFramesCount[Type] = 4;
			NPCID.Sets.AttackFrameCount[Type] = 3;
			NPCID.Sets.AttackType[Type] = 0;
			NPCID.Sets.AttackTime[Type] = 34;
			NPCID.Sets.AttackAverageChance[Type] = 30;
			NPCID.Sets.HatOffsetY[Type] = 4;

			NPC.Happiness
				.SetBiomeAffection<ForestBiome>(AffectionLevel.Like)
				.SetBiomeAffection<DesertBiome>(AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.ArmsDealer, AffectionLevel.Like)
				.SetNPCAffection(NPCID.WitchDoctor, AffectionLevel.Like)
				.SetNPCAffection(NPCID.TaxCollector, AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.Merchant, AffectionLevel.Dislike);

			// Replace liking Forest with liking Sky if config option to separate Sky and Hell into distinct biomes is enabled
			if (GetInstance<Configs.ServerConfig>().ShuffleBiomePreferences)
			{
				NPC.Happiness
					.SetBiomeAffection<ForestBiome>(0)
					.SetBiomeAffection<CustomShoppingBiomes.SkyBiome>(AffectionLevel.Like);
			}

			Profile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIdx)
			);
		}

		public override void TowneeSetDefaults()
		{
			AnimationType = NPCID.Merchant;
		}

		public override void Load()
		{
			ShimmerHeadIdx = Mod.AddNPCHeadTexture(Type, Texture + "_HeadShimmer");
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
				new FlavorTextBestiaryInfoElement("Mods.MoreTownsfolk.Bestiary.Ninja")
			});
		}

		public override ITownNPCProfile TownNPCProfile()
		{
			return Profile;
		}

		public override void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects)
		{
			position += new Vector2(0 * NPC.direction, NPC.IsShimmerVariant ? 0 : 0);
		}

		public override List<string> SetNPCNameList()
		{
			List<string> names = new();

			foreach (LocalizedText text in Language.FindAll(Lang.CreateDialogFilter("Mods.MoreTownsfolk.NPCNames.Ninja")))
			{
				names.Add(text.Value);
			}

			return names;
		}

		// Spawns after he's been saved before, provided he is not currently on a hunt
		public override bool CanTownNPCSpawn(int numTownNPCs)
		{
			return TownsfolkWorld.savedNinja && TownsfolkWorld.currentNinjaHunt == -1;
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Language.GetTextValue("LegacyInterface.28"); // "Shop"
			button2 = Language.GetTextValue("Mods.MoreTownsfolk.Common.BannerButton"); // "Turn in Banner"
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shopName)
		{
			if (firstButton)
				shopName = "Shop";
			else
			{
				// TODO: Implement the Ninja's UI so players can select any quest they want
				//		 For now I'll just be using hardcoded values to test stuff
				int questIdx = 10; // Current quest being tested: Hellbat/Lava Bat Banner -> Magma Stone

				// Get the quest data from the index we're given
				// (eventually this index will come from the UI, but I haven't implemented it yet)
				BannerQuest questData = NinjaBannerQuests.Quests[questIdx];

				// If we've already done this quest and it is non-repeatable, give some unique dialogue
				if (TownsfolkWorld.completedNinjaHunts.Contains(questIdx) && !questData.Repeatable)
				{
					GetSoldMoonPhase(questData, questIdx, out string soldMoonPhase);
					Main.npcChatText = Language.GetTextValue(DialogueKey + "SpecialDialogue.HuntFail_AlreadyDone", Lang.GetItemName(questData.RewardItem), soldMoonPhase);
					return;
				}

				// We need to find out how many valid banners the player has in their inventory
				// If they don't have enough, pressing this button won't initiate a quest, after all!
				Player plr = Main.LocalPlayer;
				int totalBannersOwned = 0;

				foreach (int bannerItemID in questData.AcceptedBannerTypes)
				{
					int mult = 1;

					// Some banners are worth more than others (i.e, Pinky Banners are worth 25x more than normal Slime banners, and Lava Bat banners are worth 3x more than Hellbat banners)
					// This makes sure that is factored into the calculations
					if (questData.ValuableBanners.TryGetValue(bannerItemID, out int value))
						mult = value;

					// Only banners in the main inventory are counted
					// The Piggy Bank, Safe, Defender's Forge, and Void Bag are all ignored, even if the player has an opened Void Bag in their inventory
					totalBannersOwned += plr.CountItem(bannerItemID) * mult;
				}

				// Not enough banners, update the message box and do nothing else
				if (totalBannersOwned < questData.Cost)
				{
					Main.npcChatText = Language.GetTextValue(DialogueKey + "SpecialDialogue.HuntFail");
					return;
				}

				// Delete banners from the player's inventory until the cost has been satiated
				for (int i = totalBannersOwned; i > totalBannersOwned - questData.Cost; i--)
				{
					Item item = plr.inventory.First(it => questData.AcceptedBannerTypes.Contains(it.type));

					// Some banners are worth more than others, so we need to decrement i by more than usual
					// Since it already gets decremented by 1, we subtract 1 less than the extra value from it
					// For example, Pinky Banners will subtract 24 from i, since they're worth 25
					if (questData.ValuableBanners.TryGetValue(item.type, out int value))
					{
						i -= value - 1;

					}

					item.stack--;

					// Ran out of items in this stack, delete it
					if (item.stack == 0)
					{
						item.TurnToAir();
					}
				}

				// We'll need to mention the NPC being hunted in the dialogue, so we need its ID
				int targetNPCID = BannerItemToNPC(questData.AcceptedBannerTypes[0]);

				// Calculate how many days it should take to return
				// Every 24 hours that have to pass adds an extra day (so 0-24 = 1 day, 25-48 = 2 days, etc.)
				// The ninja always returns the corresponding number of hours after the current time
				int huntHours = (int)Math.Round(questData.HuntTime * 24);
				float currentTime = Utils.GetDayTimeAs24FloatStartingFromMidnight(); // Examples: 6:30 am would be 6.5f, 6:00 pm would be 18.0f
				TownsfolkWorld.daysUntilReturn = (int)Math.Ceiling(huntHours / 24.0f);
				TownsfolkWorld.ninjaReturnTime = (currentTime + huntHours) % 24; // % 24 (or mod 24) wraps the value back around if it goes above 24

				// This variable needs to be set depending on whether we've passed the target time or not
				// If we don't set it properly, the Ninja will either return too early or take a whole extra day
				TownsfolkWorld.decrementedNinjaDaysToday = currentTime >= TownsfolkWorld.ninjaReturnTime;

				// Set the current hunt to this quest's index, so we can access its data when the Ninja returns
				TownsfolkWorld.currentNinjaHunt = questIdx;
				Main.npcChatText = Language.GetTextValue(DialogueKey + "SpecialDialogue.HuntLeaving", Lang.GetNPCName(targetNPCID), (int)Math.Round(24 * questData.HuntTime));
			}
		}

		public override void AddShops()
		{
			var npcShop = new NPCShop(Type, "Shop")
				// By default, only sells Ninja armor
				.Add(ItemID.NinjaHood)
				.Add(ItemID.NinjaShirt)
				.Add(ItemID.NinjaPants)
			;

			// Loop through all the loaded quests, adding the reward items if they are not repeatable quests
			// The moon phase counter is needed to determine the current moon phase- it needs to be a separate variable
			// so that repeatable quests don't cause certain moon phases to be skipped
			// It starts at -1 instead of 0 since the incrementing happens before any calculations are done
			int moonPhaseCounter = -1;

			for (int i = 0; i < NinjaBannerQuests.Quests.Count; i++)
			{
				BannerQuest quest = NinjaBannerQuests.Quests[i];

				// Ignore repeatable quests
				if (quest.Repeatable)
					continue;

				// Increment moon phase counter, so that each item cycles through the moon phases
				// This is done to prevent the Shinobi's shop from filling up if too many quests are completed
				moonPhaseCounter++;

				var targetPhase = (MoonPhase)(moonPhaseCounter % 8); // Returns a number 0-7, which is then converted to a moon phase

				// Get the condition variable from the target moon phase, so that the item is only sold on the correct moon phase
				Condition phaseCondition = targetPhase switch
				{
					MoonPhase.ThreeQuartersAtLeft => Condition.MoonPhaseWaxingCrescent,
					MoonPhase.HalfAtLeft => Condition.MoonPhaseFirstQuarter,
					MoonPhase.QuarterAtLeft => Condition.MoonPhaseWaxingGibbous,
					MoonPhase.Empty => Condition.MoonPhaseNew,
					MoonPhase.QuarterAtRight => Condition.MoonPhaseWaningGibbous,
					MoonPhase.HalfAtRight => Condition.MoonPhaseThirdQuarter,
					MoonPhase.ThreeQuartersAtRight => Condition.MoonPhaseWaningCrescent,
					_ => Condition.MoonPhaseFull,
				};

				// Add the reward item, with the condition that the corresponding quest has been completed and the moon phase is right
				npcShop.Add(new NPCShop.Entry(quest.RewardItem, ExtraConditions_MoreTownsfolk.CompletedANinjaQuest(i), phaseCondition));
			}

			npcShop.Register();
		}

		public override string GetChat()
		{
			if (TownsfolkWorld.currentNinjaHunt > -1)
			{
				// No matter what we're doing, we will need quest data
				// Since currentNinjaHunt stores an index, we can use that to find the data of our current quest
				// This only works if the current index is within range, though! If it's not, we go with our backup option,
				// resetting the quest variables and using normal dialogue
				if (TownsfolkWorld.currentNinjaHunt > NinjaBannerQuests.Quests.Count)
				{
					Mod.Logger.Error($"Ninja had a quest with index {TownsfolkWorld.currentNinjaHunt}, which is beyond the bounds of the quests list!");
					TownsfolkWorld.currentNinjaHunt = -1;
					return base.GetChat();
				}
				
				BannerQuest questData = NinjaBannerQuests.Quests[TownsfolkWorld.currentNinjaHunt];

				// Just in case the quest we got was invalid, we should check that we got a valid item ID
				// An item ID of 0 or below is not a valid item, so that calls for failsafe code again
				if (questData.RewardItem <= 0)
				{
					Mod.Logger.Error($"Ninja failed to get a valid reward item ID for quest with index {TownsfolkWorld.currentNinjaHunt}!");
					TownsfolkWorld.currentNinjaHunt = -1;
					return base.GetChat();
				}

				// Since we know we have a valid quest now, we can grab some extra data
				// Both dialogues mention the enemy being hunted, which we'll just assume is the NPC associated with the first valid banner for this quest
				int npcID = BannerItemToNPC(questData.AcceptedBannerTypes[0]);

				// Make sure the ID is valid, i.e not 0
				// IDs below 0 are ok, since those are net IDs for NPCs like Black Slimes
				if (npcID == 0)
				{
					Mod.Logger.Error($"Ninja failed to get a valid NPC ID from {Lang.GetItemNameValue(questData.AcceptedBannerTypes[0])} (Item ID {questData.AcceptedBannerTypes[0]})! Got NPC ID {npcID} instead.");
					TownsfolkWorld.currentNinjaHunt = -1;
					return base.GetChat();
				}

				// Get the NPC's name from their ID
				string npcName = Lang.GetNPCNameValue(npcID);

				// Currently trying to leave for a hunt, use leaving dialogue
				if (TownsfolkWorld.daysUntilReturn > 0)
				{
					// The Ninja mentions how long he'll be gone for, which means we need the hunt time in hours
					// The HuntTime variable in the quest data is basically just a multiplier for how many days he'll be gone
					// Most of the time this will just be 24 hours (so a multiplier of 1), but some repeatable quests have a much shorter delay
					int huntTime = (int)Math.Round(questData.HuntTime * 24);

					return Language.GetTextValue(DialogueKey + "SpecialDialogue.HuntLeaving", npcName, huntTime);
				}
				// Just got back from a hunt, give the player their reward item, add the idx of this hunt to the list of completed ones,
				// and return special dialogue and reset the hunt variable
				else
				{
					// First, we can spawn an item on the player for them to collect
					var src = NPC.GetSource_GiftOrReward();
					Main.LocalPlayer.QuickSpawnItem(src, questData.RewardItem, questData.RewardQuantity);

					// Next, we grab the localization key for the type of hunt we did; this defaults to repeatable, but is set to one-time below
					string questType = "Repeatable";
					string soldMoonPhase = "";

					// We also have to register that this quest has been fulfilled if it was a one-time quest, as well as use a different line of dialogue
					// One-time quests also need to mention the moon phase
					if (!questData.Repeatable)
					{
						TownsfolkWorld.completedNinjaHunts.Add(TownsfolkWorld.currentNinjaHunt);
						questType = "OneTime";
						GetSoldMoonPhase(questData, TownsfolkWorld.currentNinjaHunt, out soldMoonPhase);
					}

					// Lastly, we need to reset the world's variables so that the game doesn't think we're still on a quest
					// We do this last since we need to access some of the values before inputting the dialogue
					TownsfolkWorld.currentNinjaHunt = -1;
					TownsfolkWorld.ninjaReturnTime = -1;
					TownsfolkWorld.daysUntilReturn = -1;
					TownsfolkWorld.decrementedNinjaDaysToday = false;
					return Language.GetTextValue(DialogueKey + "SpecialDialogue.HuntReturn_" + questType, npcName, soldMoonPhase);
				}
			}

			// Otherwise just uses dialogue normally
			return base.GetChat();
		}

		public static MoonPhase GetSoldMoonPhase(BannerQuest questData, int questIdx, out string phaseName)
		{
			// Determine the moon phase in which this item will be sold
			// Because repeatable quests are skipped for determining the moon phase, we can't just use the quest idx directly,
			// and instead have to loop through the quests until we find this one
			// We can at least skip this loop if the quest is repeatable, since we know it isn't sold
			phaseName = "";
			int moonPhaseCounter = -1;

			if (!questData.Repeatable)
			{
				for (int i = 0; i <= questIdx; i++)
				{
					var otherQuestData = NinjaBannerQuests.Quests[i];

					if (!otherQuestData.Repeatable)
						moonPhaseCounter++;
				}

				var targetPhase = (MoonPhase)(moonPhaseCounter % 8); // Returns a number 0-7, which is then converted to a moon phase

				// Grab the name of the phase
				phaseName = GetMoonPhaseName(targetPhase);
				return targetPhase;
			}

			return (MoonPhase)(-1);
		}

		public static string GetMoonPhaseName(MoonPhase targetPhase)
		{
			return targetPhase switch
			{
				MoonPhase.Full => "full",
				MoonPhase.ThreeQuartersAtLeft => "waxing crescent",
				MoonPhase.HalfAtLeft => "first quarter",
				MoonPhase.QuarterAtLeft => "waxing gibbous",
				MoonPhase.Empty => "new",
				MoonPhase.QuarterAtRight => "waning gibbous",
				MoonPhase.HalfAtRight => "third quarter",
				MoonPhase.ThreeQuartersAtRight => "waning crescent",
				_ => null
			};
		}

		public static int BannerItemToNPC(int itemID)
		{
			Item itemData = ContentSamples.ItemsByType[itemID];
			int npcID = -1;

			// First, try using this method if it returns a valid NPC ID
			if (NPCLoader.BannerItemToNPC(itemID) != -1)
			{
				npcID = NPCLoader.BannerItemToNPC(itemID);
			}
			// Failing that, we probably have a vanilla banner, so we can calculate the banner ID from the item's placeStyle
			else if (itemData.createTile == TileID.Banners)
			{
				int bannerID = itemData.placeStyle - 21;

				npcID = Item.BannerToNPC(bannerID);
			}

			return npcID;
		}

		// Despawn the Shinobi if he should be on a quest
		// When he despawns this way, he should also spawn smoke and a decoy
		public override bool PreAI()
		{
			// Don't try to despawn the Shinobi while he's being talked to
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.talkNPC == NPC.whoAmI)
					return base.PreAI();
			}

			// Make sure the Shinobi is actually on a hunt; he shouldn't despawn if not on a hunt
			// He also shouldn't despawn when he's returned from a hunt
			if (TownsfolkWorld.currentNinjaHunt > -1 && TownsfolkWorld.daysUntilReturn > 0)
			{
				// Spawns black smoke and a custom "decoy" gore
				NinjaVanish(Vector2.UnitY * -4f);

				// Display a message about his departure
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.NewText(Language.GetTextValue("LegacyMisc.35", NPC.FullName), ChatColors.NPCArrived); // "(name) the Ninja has departed!"
				}
				else if (Main.netMode == NetmodeID.Server)
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.35", NPC.GetFullNetName()), ChatColors.NPCArrived);
				}

				// Despawn the Shinobi
				NPC.active = false;

				// Store the location of the Shinobi's home, so he'll respawn there
				// If the Ninja is homeless when he despawns, he'll respawn at the world spawn instead
				if (NPC.homeless)
				{
					TownsfolkWorld.ninjaHomeX = Main.spawnTileX;
					TownsfolkWorld.ninjaHomeY = Main.spawnTileY;
				}
				else
				{
					TownsfolkWorld.ninjaHomeX = NPC.homeTileX;
					TownsfolkWorld.ninjaHomeY = NPC.homeTileY;
				}

				return false;
			}

			return base.PreAI();
		}

		void NinjaVanish(Vector2 matVelocity)
		{
			// Spawn a cloud of black smoke, similar to what happens when a Wraith dies
			// In fact, this is exactly what happens when a Wraith dies! This code was adapted from the Wraith's dying code,
			// NPC.cs lines 91225-91241
			for (int i = 0; i < 20; i++)
			{
				int num738 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Wraith, Alpha: 50, Scale: 1.5f);
				Dust dust = Main.dust[num738];
				dust.velocity *= 2f;
				Main.dust[num738].noGravity = true;
			}

			for (int i = 0; i < 5; i++)
			{
				// Fun fact: Despite the fact there's a "GoreID" class, very few of the gores in the game actually use it!
				float goreOffset = 10f + (5f * i);
				Gore smoke = Gore.NewGoreDirect(NPC.GetSource_Death(), new Vector2(NPC.Center.X, NPC.Center.Y - goreOffset), Vector2.UnitX * Main.rand.Next(-1, 2), 99, NPC.scale);
				smoke.velocity *= 0.3f;
			}

			// In addition to the Wraith smoke, spawn a folded-up tatami mat to serve as a "decoy" that lingers for five seconds (half as long as standard gores)
			Gore mat = Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, matVelocity, GoreType<ShinobiDecoy>());
			mat.timeLeft = Conversions.ToFrames(5);
			mat.velocity = matVelocity;
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback)
		{
			damage = 12;
			knockback = 0f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
		{
			cooldown = 30;
			randExtraCooldown = 10;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
		{
			projType = ProjectileID.Shuriken;
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
		{
			multiplier = 9;
		}
	}
}