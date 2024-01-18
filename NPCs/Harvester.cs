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
	public class Harvester : ModTownee
	{
		//private static int ShimmerHeadIdx;
		private static Profiles.StackedNPCProfile Profile;

		public override string DialogueKey => "Mods.MoreTownsfolk.Dialogue.Harvester.";
		public override bool IsMale => false;

		public override void TowneeStaticDefaults()
		{
			Main.npcFrameCount[Type] = 25;

			NPCID.Sets.ExtraFramesCount[Type] = 5;
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.AttackType[Type] = 0;
			NPCID.Sets.AttackTime[Type] = 45;
			NPCID.Sets.AttackAverageChance[Type] = 30;

			NPC.Happiness
				.SetBiomeAffection<CrimsonBiome>(AffectionLevel.Like)
				.SetBiomeAffection<DesertBiome>(AffectionLevel.Dislike)
				.SetBiomeAffection<HallowBiome>(AffectionLevel.Hate)
				.SetNPCAffection(NPCType<Occultist>(), AffectionLevel.Love)
				.SetNPCAffection(NPCID.Nurse, AffectionLevel.Like)
				.SetNPCAffection(NPCID.Pirate, AffectionLevel.Like)
				.SetNPCAffection(NPCID.Guide, AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.Dryad, AffectionLevel.Hate);

			Profile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party")//,
				//new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIdx, Texture + _ShimmerParty)
			);
		}

		public override void TowneeSetDefaults()
		{
			AnimationType = NPCID.Merchant;
		}

		//public override void Load()
		//{
			//ShimmerHeadIdx = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
		//}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
				new FlavorTextBestiaryInfoElement("Mods.MoreTownsfolk.Bestiary.Harvester")
			});
		}

		public override ITownNPCProfile TownNPCProfile()
		{
			return Profile;
		}

		public override List<string> SetNPCNameList()
		{
			List<string> names = new();

			foreach (LocalizedText text in Language.FindAll(Lang.CreateDialogFilter("Mods.MoreTownsfolk.NPCNames.Harvester")))
			{
				names.Add(text.Value);
			}

			return names;
		}

		// Spawns in Hardmode if at least one block of Corruption exists
		public override bool CanTownNPCSpawn(int numTownNPCs)
		{
			if (Main.hardMode)
			{
				var tileCounts = new int[TileLoader.TileCount];
				WorldGen.CountTileTypesInArea(tileCounts, 0, Main.maxTilesX, 0, Main.maxTilesY);
				tileCounts[TileID.Sunflower] = 0;
				if (WorldGen.GetTileTypeCountByCategory(tileCounts, TileScanGroup.Corruption) > 0)
				{
					return true;
				}
			}

			return false;
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
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shopName)
		{
			if (firstButton)
				shopName = "Shop";
		}

		public override void AddShops()
		{
			var npcShop = new NPCShop(Type, "Shop")
				.Add(ItemID.CrimsonSeeds)
				.Add(ItemType<TeleportationPylonCrimson>(), Condition.HappyEnoughToSellPylons)
			;

			npcShop.Register();
		}

		public override string GetChat()
		{
			// Has a 30% chance to return a special dialogue if your world has no Crimson
			var tileCounts = new int[TileLoader.TileCount];
			WorldGen.CountTileTypesInArea(tileCounts, 0, Main.maxTilesX, 0, Main.maxTilesY);
			tileCounts[TileID.Sunflower] = 0;
			if (WorldGen.GetTileTypeCountByCategory(tileCounts, TileScanGroup.Crimson) <= 0 && Main.rand.NextFloat() <= 0.3f)
			{
				return Language.GetTextValue("Mods.MoreTownsfolk.Dialogue.Harvester.Dialogue21").Replace("{?Day}{?!Day}", "");
			}

			// Otherwise, just returns default dialogue
			return base.GetChat();
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback)
		{
			damage = 30;
			knockback = 6.5f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
		{
			cooldown = 30;
			randExtraCooldown = 10;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
		{
			projType = ProjectileType<HarvesterAttack>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
		{
			multiplier = 0;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			foreach (Projectile proj in Main.projectile.Where(p => p.type == ProjectileType<HarvesterAttack>()))
			{
				if (proj.owner == NPC.whoAmI)
				{
					proj.Kill();
				}
			}
		}

		// 1/10 chance to drop "The Hook", a melee weapon
		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(new CommonDrop(ItemType<TheHook>(), 10));
		}
	}
}