using Humanizer;
using MoreTownsfolk.Items;
using MoreTownsfolk.Projectiles;
using Steamworks;
using TepigCore.Base.ModdedNPC;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;

namespace MoreTownsfolk.NPCs
{
	[AutoloadHead]
	public class Occultist : ModTownee
	{
		private static int ShimmerHeadIdx;
		private static Profiles.StackedNPCProfile Profile;

		public override string DialogueKey => "Mods.MoreTownsfolk.Dialogue.Occultist.";
		public override bool IsMale => false;

		public override void TowneeStaticDefaults()
		{
			Main.npcFrameCount[Type] = 23;

			NPCID.Sets.ExtraFramesCount[Type] = 4;
			NPCID.Sets.AttackFrameCount[Type] = 3;
			NPCID.Sets.AttackType[Type] = 2;
			NPCID.Sets.AttackTime[Type] = 45;
			NPCID.Sets.AttackAverageChance[Type] = 30;
			NPCID.Sets.HatOffsetY[Type] = 4;

			NPC.Happiness
				.SetBiomeAffection<CorruptionBiome>(AffectionLevel.Like)
				.SetBiomeAffection<JungleBiome>(AffectionLevel.Dislike)
				.SetBiomeAffection<HallowBiome>(AffectionLevel.Hate)
				.SetNPCAffection(NPCType<Harvester>(), AffectionLevel.Love)
				.SetNPCAffection(NPCID.TaxCollector, AffectionLevel.Like)
				.SetNPCAffection(NPCID.Clothier, AffectionLevel.Like)
				.SetNPCAffection(NPCID.Guide, AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.Dryad, AffectionLevel.Hate);

			Profile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIdx)
			);
		}

		public override void TowneeSetDefaults()
		{
			AnimationType = NPCID.Wizard;
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
				new FlavorTextBestiaryInfoElement("Mods.MoreTownsfolk.Bestiary.Occultist")
			});
		}

		public override ITownNPCProfile TownNPCProfile()
		{
			return Profile;
		}

		public override List<string> SetNPCNameList()
		{
			List<string> names = new();

			foreach (LocalizedText text in Language.FindAll(Lang.CreateDialogFilter("Mods.MoreTownsfolk.NPCNames.Occultist")))
			{
				names.Add(text.Value);
			}

			return names;
		}

		// Spawns when the Brain of Cthulhu has been defeated
		public override bool CanTownNPCSpawn(int numTownNPCs)
		{
			return TownsfolkWorld.downedBrain;
		}

		// Prevent housing in areas with too many "good" tiles
		public override bool CheckConditions(int left, int right, int top, int bottom)
		{
			var tileCounts = new int[TileLoader.TileCount];
			WorldGen.CountTileTypesInArea(tileCounts, left, right, top, bottom);
			if (WorldGen.GetTileTypeCountByCategory(tileCounts, TileScanGroup.TotalGoodEvil) > 0)
			{
				return false;
			}
			
			return base.CheckConditions(left, right, top, bottom);
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Language.GetTextValue("LegacyInterface.28"); // "Shop"
			button2 = Language.GetTextValue("Mods.MoreTownsfolk.Common.CorruptButton"); // "Corrupt Armor"
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shopName)
		{
			if (firstButton)
				shopName = "Shop";
			else
			{
				Main.playerInventory = true;
				OccultUI.NPCIndex = NPC.whoAmI;
				OccultUI.CurrentlyViewing = true;
			}
		}

		public override void AddShops()
		{
			var npcShop = new NPCShop(Type, "Shop")
				// Corrupt Seeds and Grass Walls
				.Add(ItemID.CorruptSeeds)
				.Add(ItemID.CorruptGrassEcho)

				// Shadow Orb items and Demonite (dependent on moon phase)
				.Add(ItemID.DemoniteOre, Condition.MoonPhaseFull)
				.Add(ItemID.Musket, Condition.MoonPhaseWaningGibbous)
				.Add(ItemID.BallOHurt, Condition.MoonPhaseThirdQuarter)
				.Add(ItemID.Vilethorn, Condition.MoonPhaseWaningCrescent)
				.Add(ItemID.BandofStarpower, Condition.MoonPhaseNew)
				.Add(ItemID.ShadowOrb, Condition.MoonPhaseWaxingCrescent)

				// Corruption Pylon
				.Add(ItemType<TeleportationPylonCorruption>(), Condition.HappyEnoughToSellPylons)
			;

			if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
			{
				if (thorium.TryFind("FanLetter2", out ModItem fanLetter) && thorium.TryFind("DarkHeart", out ModItem darkHeart))
				{
					npcShop
						.Add(fanLetter.Type, Condition.MoonPhaseFirstQuarter)
						.Add(darkHeart.Type, Condition.MoonPhaseWaxingGibbous)
					;
				}
				else
				{
					npcShop
						.Add(ItemID.DemoniteOre, Condition.MoonPhaseFirstQuarter)
						.Add(ItemID.DemoniteOre, Condition.MoonPhaseWaxingGibbous)
					;
				}
			}
			else
			{
				npcShop
					.Add(ItemID.DemoniteOre, Condition.MoonPhaseFirstQuarter)
					.Add(ItemID.DemoniteOre, Condition.MoonPhaseWaxingGibbous)
				;
			}

			npcShop.Register();
		}

		public override string GetChat()
		{
			// Has a 30% chance to return a special dialogue if your world has no Corruption
			var tileCounts = new int[TileLoader.TileCount];
			WorldGen.CountTileTypesInArea(tileCounts, 0, Main.maxTilesX, 0, Main.maxTilesY);
			tileCounts[TileID.Sunflower] = 0;
			if (WorldGen.GetTileTypeCountByCategory(tileCounts, TileScanGroup.Corruption) <= 0 && Main.rand.NextFloat() <= 0.3f)
			{
				return Language.GetTextValue("Mods.MoreTownsfolk.Dialogue.Occultist.Dialogue23").Replace("{?Day}{?!Day}", "");
			}

			// Otherwise, just returns default dialogue
			return base.GetChat();
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback)
		{
			damage = 12;
			knockback = 1.5f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
		{
			cooldown = 30;
			randExtraCooldown = 10;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
		{
			projType = ProjectileType<OccultistAttack>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
		{
			multiplier = 0;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			foreach (Projectile proj in Main.projectile.Where(p => p.type == ProjectileType<OccultistAttack>()))
			{
				if (proj.owner == NPC.whoAmI)
				{
					proj.Kill();
				}
			}
		}

		// 1/10 chance to drop "Spittle", a magic weapon
		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(new CommonDrop(ItemType<Spittle>(), 10));
		}

		public override void AI()
		{
			base.AI();

			// Check if player is talking to this Occultist while on GFB seed
			if (Main.LocalPlayer.TalkNPC != null && Main.LocalPlayer.TalkNPC.whoAmI == NPC.whoAmI && Main.zenithWorld)
			{
				// If the Occultist's secret hasn't been triggered yet, check if it should be triggered
				if (!TownsfolkWorld.occultistSecret)
				{
					string translatedText = Language.GetTextValue("Mods.MoreTownsfolk.Dialogue.Occultist.Dialogue14").Replace("{?BloodMoon}", "");

					// The dialogue about placing a block of Ebonstone was used, secret activated
					if (Main.npcChatText.Equals(translatedText))
					{
						List<Tile> validTiles = new();

						// Get the position of the player's spawn point
						Vector2 spawn = new(Main.LocalPlayer.SpawnX, Main.LocalPlayer.SpawnY);

						if (spawn.X == -1 || spawn.Y == -1)
						{
							spawn = new Vector2(Main.spawnTileX, Main.spawnTileY);
						}

						// Limit the placement of the block to within 16 blocks of spawn horizontally, and 1-8 blocks below it vertically
						int startX = (int)Math.Max(spawn.X - 16, 0);
						int endX = (int)Math.Min(spawn.X + 16, Main.maxTilesX);
						int startY = (int)Math.Max(spawn.Y + 1, 0);
						int endY = (int)Math.Min(spawn.Y + 8, Main.maxTilesY);

						// Find a random block within range that is a tile that's not already Ebonstone
						int failsafeTimer = 100;
						Vector2 chosenPos = new(-1, -1);
						while (chosenPos == new Vector2(-1, -1))
						{
							int i = Main.rand.Next(startX, endX);
							int j = Main.rand.Next(startY, endY);
							Tile t = Main.tile[i, j];

							if (t.TileType != TileID.Ebonstone)
							{
								chosenPos = new Vector2(i, j);
							}

							// If we test 100 tiles and can't find a valid tile, skip placing a tile at all
							// This prevents the dialogue from ever freezing the game
							if (failsafeTimer-- < 0)
								return;
						}

						// The random tile is then converted to Ebonstone
						Tile tile = Main.tile[chosenPos.ToPoint()];
						tile.ResetToType(TileID.Ebonstone);

						// Make sure the game knows the secret has been triggered now
						TownsfolkWorld.occultistSecret = true;
					}
				}
			}
		}
	}
}