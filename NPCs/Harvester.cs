using MoreTownsfolk.Items;
using MoreTownsfolk.Projectiles;
using TepigCore.Base.ModdedNPC;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.UI;

namespace MoreTownsfolk.NPCs
{
	[AutoloadHead]
	public class Harvester : ModTownee
	{
		private static int ShimmerHeadIdx;
		private static Profiles.StackedNPCProfile Profile;

		public override string DialogueKey => "Mods.MoreTownsfolk.NPCs.Harvester.";
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
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
				new FlavorTextBestiaryInfoElement("Mods.MoreTownsfolk.Bestiary.Harvester")
			});
		}

		public override ITownNPCProfile TownNPCProfile()
		{
			return Profile;
		}

		public override void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects)
		{
			position += new Vector2(-4 * NPC.direction, NPC.IsShimmerVariant ? 2 : 0);
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

		// Spawns once the Eater of Worlds has been defeated
		public override bool CanTownNPCSpawn(int numTownNPCs)
		{
			return TownsfolkWorld.downedEater;
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

		public static int crittersGiven = 0;
		static readonly int MAX_CRITTERS = 10;

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Language.GetTextValue("LegacyInterface.28"); // "Shop"
			button2 = Language.GetTextValue("Mods.MoreTownsfolk.Common.HarvestButton") + $"({crittersGiven}/{MAX_CRITTERS})"; // "Give Critter (#/10)"
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shopName)
		{
			if (firstButton)
				shopName = "Shop";
			else
			{
				Player player = Main.LocalPlayer;

				if (!player.inventory.Any(i => !i.favorited && i.stack > 0 && i.makeNPC > 0)) // Player has no usable critters in their inventory
				{
					Main.npcChatText = Language.GetTextValue(DialogueKey + "ExtraDialogue.NoCritters" + Main.rand.Next(3));
				}
				else
				{
					foreach (Item item in player.inventory)
					{
						if (item.favorited || item.stack <= 0)
							continue;

						// Item is most likely a critter (theoretically it could be a captured NPC from Fargo's though)
						// That's why we have to check that the item isn't from Fargo's Mutant Mod
						// I don't *think* Mutant Mod adds any critters (Top Hat Squirrels are from Souls Mod) so no need to worry about that
						if (item.makeNPC > 0 && (item.ModItem == null || item.ModItem.Mod.Name.Equals("Fargowiltas")))
						{
							SoundEngine.PlaySound(SoundID.Grab);
							int crittersLeft = MAX_CRITTERS - crittersGiven;

							// While holding shift, give enough critters to reach the requirement
							if (ItemSlot.ShiftInUse && item.stack >= crittersLeft)
							{
								item.stack -= crittersLeft;
								crittersGiven += crittersLeft;
							}
							// If the player doesn't have enough critters in their inventory to do that,
							// use as many critters as the player has
							else if (ItemSlot.ShiftInUse)
							{
								crittersGiven += item.stack;
								item.stack = 0;
							}
							else
							{
								item.stack--;
								crittersGiven++;
							}

							if (item.stack <= 0)
								item.TurnToAir();

							if (crittersGiven >= MAX_CRITTERS)
							{
								crittersGiven = 0;
								Main.npcChatText = Language.GetTextValue(DialogueKey + "ExtraDialogue.GaveFood" + Main.rand.Next(3));
								var source = player.GetSource_GiftOrReward();

								List<int> preBoss = new()
								{
									ItemID.BananaSplit,
									ItemID.Burger,
									ItemID.MilkCarton,
									ItemID.ChickenNugget,
									ItemID.CoffeeCup,
									ItemID.FriedEgg,
									ItemID.Fries,
									ItemID.Hotdog,
									ItemID.IceCream,
									ItemID.Nachos,
									ItemID.Pizza,
									ItemID.PotatoChips,
									ItemID.ShrimpPoBoy,
									ItemID.Spaghetti,
									ItemID.Steak
								};

								List<int> postSkele = new()
								{
									ItemID.CreamSoda
								};

								List<int> hardmode = new()
								{
									ItemID.ApplePie,
									ItemID.Bacon,
									ItemID.ChocolateChipCookie,
									ItemID.Grapes,
									ItemID.Milkshake
								};

								List<int> postPlant = new()
								{
									ItemID.BBQRibs
								};

								if (NPC.downedBoss3)
									preBoss.AddRange(postSkele);

								if (Main.hardMode)
									preBoss.AddRange(hardmode);

								if (NPC.downedPlantBoss)
									preBoss.AddRange(postPlant);

								player.QuickSpawnItem(source, preBoss[Main.rand.Next(preBoss.Count)]);
								SoundEngine.PlaySound(SoundID.Chat);
							}

							break;
						}
					}
				}
			}
		}

		public override void AddShops()
		{
			var npcShop = new NPCShop(Type, "Shop")
				// Crimson Seeds and Grass Walls
				.Add(ItemID.CrimsonSeeds)
				.Add(ItemID.CrimsonGrassEcho)
				
				// Crimson Heart items and Crimtane (dependent on moon phase)
				.Add(ItemID.CrimtaneOre, Condition.MoonPhaseFull)
				.Add(ItemID.TheUndertaker, Condition.MoonPhaseWaningGibbous)
				.Add(ItemID.TheRottedFork, Condition.MoonPhaseThirdQuarter)
				.Add(ItemID.CrimsonRod, Condition.MoonPhaseWaningCrescent)
				.Add(ItemID.PanicNecklace, Condition.MoonPhaseNew)
				.Add(ItemID.CrimsonHeart, Condition.MoonPhaseWaxingCrescent)
				
				// Crimson Pylon
				.Add(ItemType<TeleportationPylonCrimson>(), Condition.HappyEnoughToSellPylons)
			;

			if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
			{
				if (thorium.TryFind("FanLetter", out ModItem fanLetter) && thorium.TryFind("DarkHeart", out ModItem darkHeart))
				{
					npcShop
						.Add(fanLetter.Type, Condition.MoonPhaseFirstQuarter)
						.Add(darkHeart.Type, Condition.MoonPhaseWaxingGibbous)
					;
				}
				else
				{
					npcShop
						.Add(ItemID.CrimtaneOre, Condition.MoonPhaseFirstQuarter)
						.Add(ItemID.CrimtaneOre, Condition.MoonPhaseWaxingGibbous)
					;
				}
			}
			else
			{
				npcShop
					.Add(ItemID.CrimtaneOre, Condition.MoonPhaseFirstQuarter)
					.Add(ItemID.CrimtaneOre, Condition.MoonPhaseWaxingGibbous)
				;
			}

			npcShop.Register();
		}

		public override string GetChat()
		{
			// First, has a 1/8 chance during a Blood Moon on GFB to use special super-secret dialogue
			if (Main.zenithWorld && Main.bloodMoon && Main.rand.NextFloat() <= 0.125f)
			{
				var source = new Terraria.DataStructures.PlayerDeathReason()
				{
					CustomReason = NetworkText.FromKey(DialogueKey + "ExtraDialogue.PlayerDeath_Harvested", Main.LocalPlayer.name)
				};

				var hurtInfo = new Player.HurtInfo()
				{
					Damage = 666,
					Dodgeable = false,
					DamageSource = source
				};

				Main.LocalPlayer.Hurt(hurtInfo);

				CombatText.NewText(NPC.getRect(), Color.Crimson, Language.GetTextValue(DialogueKey + "ExtraDialogue.GfbKill"), true);

				return Language.GetTextValue(DialogueKey + "ExtraDialogue.GfbKill");
			}

			// If that fails for any reason, has a 30% chance to return a special dialogue if your world has no Crimson
			// This special dialogue gives the player five Crimson Seeds (and won't trigger if Crimson Seeds exist in the player's inventory)
			var tileCounts = new int[TileLoader.TileCount];
			WorldGen.CountTileTypesInArea(tileCounts, 0, Main.maxTilesX, 0, Main.maxTilesY);
			tileCounts[TileID.Sunflower] = 0;
			if (WorldGen.GetTileTypeCountByCategory(tileCounts, TileScanGroup.Crimson) <= 0 && !Main.LocalPlayer.HasItem(ItemID.CrimsonSeeds) && Main.rand.NextFloat() <= 0.3f)
			{
				Main.LocalPlayer.QuickSpawnItem(NPC.GetSource_FromThis(), ItemID.CrimsonSeeds, 5);
				return Language.GetTextValue(DialogueKey + "Dialogue.Dialogue14").Replace("{?Day}{?!Day}", "");
			}

			// Failing both of those, if Blood and Gore is enabled and the Pirate is present, 30% chance to return Pirate dialogue
			if (ChildSafety.Disabled && NPC.AnyNPCs(NPCID.Pirate) && Main.rand.NextFloat() <= 0.3f)
			{
				return Language.GetTextValue(DialogueKey + "Dialogue.Dialogue9").Replace("{?Day}{?!Day}", "").Replace("{Pirate}", NPC.GetFirstNPCNameOrNull(NPCID.Pirate));
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