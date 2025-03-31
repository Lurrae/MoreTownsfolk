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
				int questIdx = 0; // Current quest being tested: Any slime banner -> Gel (x100) (repeatable)

				// Get the quest data from the index we're given
				// (eventually this index will come from the UI, but I haven't implemented it yet)
				BannerQuest questData = NinjaBannerQuests.Quests[questIdx];

				// If we've already done this quest and it is non-repeatable, give some unique dialogue
				if (TownsfolkWorld.completedNinjaHunts.Contains(questIdx) && !questData.Repeatable)
				{
					Main.npcChatText = Language.GetTextValue(DialogueKey + "SpecialDialogue.HuntFail_AlreadyDone", Lang.GetItemName(questData.RewardItem));
					return;
				}

				// We need to find out how many valid banners the player has in their inventory
				// If they don't have enough, pressing this button won't initiate a quest, after all!
				Player plr = Main.LocalPlayer;
				int totalBannersOwned = 0;

				foreach (int bannerItemID in questData.AcceptedBannerTypes)
				{
					// Only banners in the main inventory are counted
					// The Piggy Bank, Safe, Defender's Forge, and Void Bag are all ignored, even if the player has an opened Void Bag in their inventory
					totalBannersOwned += plr.CountItem(bannerItemID);
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

			// TODO: Add wares for any completed quests
			//		 This is easier said than done, since items can't be added at runtime as the quests are finished
			//		 I'll have to figure out how to make a condition that can dynamically check the completed status of any quest...

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

					// We also have to register that this quest has been fulfilled if it was a one-time quest, as well as use a different line of dialogue
					if (!questData.Repeatable)
					{
						TownsfolkWorld.completedNinjaHunts.Add(TownsfolkWorld.currentNinjaHunt);
						questType = "OneTime";
					}

					// Lastly, we need to reset the world's variables so that the game doesn't think we're still on a quest
					// We do this last since we need to access some of the values before inputting the dialogue
					TownsfolkWorld.currentNinjaHunt = -1;
					TownsfolkWorld.ninjaReturnTime = -1;
					TownsfolkWorld.daysUntilReturn = -1;
					TownsfolkWorld.decrementedNinjaDaysToday = false;
					return Language.GetTextValue(DialogueKey + "SpecialDialogue.HuntReturn_" + questType, npcName);
				}
			}

			// Otherwise just uses dialogue normally
			return base.GetChat();
		}

		private static int BannerItemToNPC(int itemID)
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

		// Despawn the Ninja if he should be on a quest and he's offscreen
		public override bool PreAI()
		{
			if (TownsfolkWorld.currentNinjaHunt > -1 && !IsNpcOnscreen(NPC.Center))
			{
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.NewText(Language.GetTextValue("LegacyMisc.35", NPC.FullName), ChatColors.NPCArrived); // "(name) the Ninja has departed!"
				}
				else if (Main.netMode == NetmodeID.Server)
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.35", NPC.GetFullNetName()), ChatColors.NPCArrived);
				}

				NPC.active = false;

				// Store the location of the Ninja's home, so he'll respawn there
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

		private static bool IsNpcOnscreen(Vector2 center)
		{
			int w = NPC.sWidth + NPC.safeRangeX * 2;
			int h = NPC.sHeight + NPC.safeRangeY * 2;
			Rectangle npcScreenRect = new((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
			foreach (Player player in Main.ActivePlayers)
			{
				// If any player is close enough to the traveling merchant, it will prevent the npc from despawning
				if (player.getRect().Intersects(npcScreenRect))
				{
					return true;
				}
			}
			return false;
		}

		// If the Ninja is supposed to be on a quest, make him try to navigate offscreen like the Traveling Merchant does at dusk
		// He won't do this if being talked to
		public override void AI()
		{
			// Do nothing while any player is talking to the Ninja
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.talkNPC == NPC.whoAmI)
					return;
			}

			if (TownsfolkWorld.currentNinjaHunt > -1 && TownsfolkWorld.daysUntilReturn > 0)
			{
				// TODO: Make the Ninja walk offscreen
				//		 For now he just has placeholder code to make him a s c e n d
				NPC.velocity.Y -= 1.0f;
				NPC.noTileCollide = true;
			}
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
			multiplier = 1;
		}
	}
}