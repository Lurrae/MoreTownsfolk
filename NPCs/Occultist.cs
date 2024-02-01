using MoreTownsfolk.Items;
using MoreTownsfolk.Projectiles;
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
		//private static int ShimmerHeadIdx;
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
				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party")//,
				//new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIdx, Texture + _ShimmerParty)
			);
		}

		public override void TowneeSetDefaults()
		{
			AnimationType = NPCID.Wizard;
		}

		//public override void Load()
		//{
			//ShimmerHeadIdx = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
		//}

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
	}
}