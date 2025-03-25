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

		public override string DialogueKey => "Mods.MoreTownsfolk.NPCs.Ninja.Dialogue.";
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
					Main.npcChatText = Language.GetTextValue("Mods.MoreTownsfolk.NPCs.Ninja.SpecialDialogue.HuntFail_AlreadyDone", Lang.GetItemName(questData.RewardItem));
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
					Main.npcChatText = Language.GetTextValue("Mods.MoreTownsfolk.NPCs.Ninja.SpecialDialogue.HuntFail");
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

				TownsfolkWorld.currentNinjaHunt = questIdx;
				// Convert this quest's HuntTime to 24-hour days, in frames
				// 3600 seconds = 1 hour, times 24 is 1 day, then that gets multiplied by the HuntTime
				TownsfolkWorld.currentNinjaHuntTimer = Conversions.ToFrames((float)Math.Round(3600 * 24 * questData.HuntTime));
				Main.npcChatText = Language.GetTextValue("Mods.MoreTownsfolk.NPCs.Ninja.SpecialDialogue.HuntLeaving", Lang.GetNPCName(targetNPCID), (int)Math.Round(24 * questData.HuntTime));
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
				if (TownsfolkWorld.currentNinjaHuntTimer > 0)
				{
					// The Ninja mentions how long he'll be gone for, which means we need the hunt time in hours
					// The HuntTime variable in the quest data is basically just a multiplier for how many days he'll be gone
					// Most of the time this will just be 24 hours (so a multiplier of 1), but some repeatable quests have a much shorter delay
					int huntTime = (int)Math.Round(questData.HuntTime * 24);

					return Language.GetTextValue("Mods.MoreTownsfolk.NPCs.Ninja.SpecialDialogue.HuntLeaving", npcName, huntTime);
				}
				// Just got back from a hunt, give the player their reward item, add the idx of this hunt to the list of completed ones,
				// and return special dialogue and reset the hunt variable
				else
				{
					// First, we can spawn an item on the player for them to collect
					var src = NPC.GetSource_GiftOrReward();
					Main.LocalPlayer.QuickSpawnItem(src, questData.RewardItem, questData.RewardQuantity);

					// Next, we register that this quest has been fulfilled if it was a one-time quest
					if (!questData.Repeatable)
					{
						TownsfolkWorld.completedNinjaHunts.Add(TownsfolkWorld.currentNinjaHunt);
					}

					// Lastly, we need to reset this variable so that the game doesn't think we're still on a quest
					// We do this last since we need to access this value before inputting the dialogue obviously
					TownsfolkWorld.currentNinjaHunt = -1;
					return Language.GetTextValue("Mods.MoreTownsfolk.NPCs.Ninja.SpecialDialogue.HuntReturn", npcName);
				}
			}

			// Otherwise just uses dialogue normally
			return base.GetChat();
		}

		private static int BannerItemToNPC(int itemID)
		{
			Item itemData = ContentSamples.ItemsByType[itemID];
			int npcID = -1;

			// For vanilla banners, we just need to get the placeStyle of the banner, which gives us a special "Banner ID" that's able to be converted
			// For some reason item IDs can't be converted directly, so this is needed
			// Only vanilla banners use the vanilla TileID, so we can check for that
			if (itemData.createTile == TileID.Banners)
			{
				// Vanilla banners use their placeStyle as their Banner ID
				npcID = Item.BannerToNPC(itemData.placeStyle);
			}
			// If the item doesn't use the vanilla tile ID, we should make 100% sure it's a modded item, then plug it in to this special method
			// that's only used for modded banners. It gives us the NPC ID given just the banner's item ID, which is nice
			else if (itemData.ModItem != null)
			{
				// Modded banners have a special method we can call
				npcID = NPCLoader.BannerItemToNPC(itemID);
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
					Main.NewText(Language.GetTextValue("LegacyMisc.35", NPC.FullName), 50, 125, 255); // "(name) the Ninja has departed!"
				}
				else if (Main.netMode == NetmodeID.Server)
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.35", NPC.GetFullNetName()), new Color(50, 125, 255));
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

			if (TownsfolkWorld.currentNinjaHunt > -1 && TownsfolkWorld.currentNinjaHuntTimer > 0)
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