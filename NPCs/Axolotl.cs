using rail;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.ID;

namespace MoreTownsfolk.NPCs
{
	[AutoloadHead]
	public class Axolotl : ModNPC
	{
		private static int HeadIdx_Blue;
		private static int HeadIdx_Copper;
		private static int HeadIdx_Gold;
		private static int HeadIdx_Melanoid;
		private static int HeadIdx_Pink;
		private static int HeadIdx_Wild;
		private static ModdedVariantNPCProfile Profile;

		public override void Load()
		{
			HeadIdx_Blue = Mod.AddNPCHeadTexture(Type, Texture + "_Blue_Head");
			HeadIdx_Copper = Mod.AddNPCHeadTexture(Type, Texture + "_Copper_Head");
			HeadIdx_Gold = Mod.AddNPCHeadTexture(Type, Texture + "_Gold_Head");
			HeadIdx_Melanoid = Mod.AddNPCHeadTexture(Type, Texture + "_Melanoid_Head");
			HeadIdx_Pink = Mod.AddNPCHeadTexture(Type, Texture + "_Pink_Head");
			HeadIdx_Wild = Mod.AddNPCHeadTexture(Type, Texture + "_Wild_Head");
		}

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 28;

			// Copy the NPCID Sets of the town cat
			// Not sure how many of these are necessary
			NPCID.Sets.IsTownPet[Type] = true;
			NPCID.Sets.TownNPCBestiaryPriority.Add(Type);
			NPCID.Sets.ExtraFramesCount[Type] = 18;
			NPCID.Sets.DangerDetectRange[Type] = 250;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
			NPCID.Sets.NPCFramingGroup[Type] = 8;

			// The player needs to stand a bit closer to the axolotl when they pet it
			NPCID.Sets.PlayerDistanceWhilePetting[Type] = 24;

			// Axolotls cannot sit on furniture
			NPCID.Sets.CannotSitOnFurniture[Type] = true;

			// Values copied from town cat's bestiary entry
			NPCID.Sets.NPCBestiaryDrawModifiers value = new() { Velocity = 0.25f };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);

			// Create all the variant textures for the axolotl
			Profile = new ModdedVariantNPCProfile(
				Texture,
				"Mods.MoreTownsfolk.NPCNames.",
				"Axolotl",
				new int[] { HeadIdx_Blue, HeadIdx_Copper, HeadIdx_Gold, HeadIdx_Melanoid, HeadIdx_Pink, HeadIdx_Wild },
				new string[] { "Blue", "Copper", "Gold", "Melanoid", "Pink", "Wild" }
			);
		}

		public override void SetDefaults()
		{
			NPC.CloneDefaults(NPCID.TownCat);
			NPC.height = 16;
			NPC.aiStyle = NPCAIStyleID.Passive;
			AnimationType = NPCID.TownCat; // Copies town cat's animation style exactly
		}

		public override void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects)
		{
			int frame = NPC.frame.Y / NPC.frame.Height;
			int xOffset = 4;
			int yOffset = 2;

			switch (frame)
			{
				case 1:
					yOffset -= 6;
					break;
				case 11:
				case 12:
				case 13:
				case 14:
					yOffset += 2;
					break;
				case 22:
				case 27:
					yOffset -= 2;
					break;
				case 23:
				case 24:
				case 25:
				case 26:
					yOffset -= 4;
					break;
				default:
					break;
			}

			position.X += xOffset * NPC.spriteDirection;
			position.Y += yOffset;

			// While sitting in a chair, the hat needs to move up a bit more
			if (NPC.ai[0] == 5f)
			{
				position.Y += -10;
			}
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("Mods.MoreTownsfolk.Bestiary.Axolotl")
			});
		}

		// Spawns once the player has purchased and used an Axolotl License
		public override bool CanTownNPCSpawn(int numTownNPCs)
		{
			return TownsfolkWorld.boughtAxolotl;
		}

		public override string GetChat()
		{
			string locKey = "Mods.MoreTownsfolk.Dialogue.Axolotl.Dialogue" + (Main.rand.NextBool() ? "1" : "2");
			return Language.GetTextValue(locKey);
		}

		public override ITownNPCProfile TownNPCProfile()
		{
			return Profile;
		}

		int swimTimer = 0;
		Rectangle frame;

		// Swimming animation
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.wet)
			{
				if (frame == default)
				{
					frame = NPC.frame;
				}
				var spriteEffects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				var rect = new Rectangle((int)(NPC.getRect().X - screenPos.X), (int)(NPC.getRect().Y - screenPos.Y), frame.Width, frame.Height);
				var origin = new Vector2(frame.Width / 2, frame.Height / 2);

				if (frame.Y < 0)
				{
					frame.Y = 0;
				}

				if (!Main.gamePaused && !Main.gameInactive) // Pause animation while game is paused or inactive
					swimTimer++;
				
				if (swimTimer > 10)
				{
					swimTimer = 0;
					frame.Y += 40;

					if (frame.Y > 3 * 40)
					{
						frame.Y = 0;
					}
				}

				spriteBatch.Draw(Request<Texture2D>(Profile.GetTexturePath(NPC) + "_Swim").Value, rect, frame, drawColor, NPC.rotation, origin, spriteEffects, 0);
				return false;
			}

			return base.PreDraw(spriteBatch, screenPos, drawColor);
		}

		// Extra AI for swimming
		int timer = 0;
		public override void PostAI()
		{
			// This AI only runs if the axolotl is in water
			if (!NPC.wet)
				return;

			// Prevent the Axolotl from drowning
			NPC.breath = NPC.breathMax;

			// Target y-velocity for bobbing up and down in the water
			float targetY = (float)Math.Sin(timer / 24);
			timer++;

			// Face the player when spoken to, even in water
			Player plr = Main.player[Main.myPlayer];

			if (plr.TalkNPC == NPC)
			{
				NPC.velocity = Vector2.Zero;
				if (NPC.DirectionTo(plr.Center).X > 0)
					NPC.direction = 1;
				else
					NPC.direction = -1;

				NPC.velocity = NPC.velocity.MoveTowards(new Vector2(0, targetY * 0.5f), 0.5f);

				return;
			}

			// Swim in current direction, and turn around when nearing the world's edges
			NPC.velocity = NPC.velocity.MoveTowards(new Vector2(4.5f * NPC.direction, targetY * 0.5f), 0.5f);

			// If near either edge of the world, turn around
			// Without this, the axolotls would be able to swim off into the sunset, never to be seen again
			// ...Until the following morning when they respawn, of course
			if (NPC.direction == -1 && NPC.position.X <= Conversions.ToPixels(45)) // Within 45 blocks of left edge
			{
				NPC.direction = 1;
			}
			else if (NPC.direction == 1 && NPC.position.X >= Conversions.ToPixels(Main.maxTilesX) - Conversions.ToPixels(45)) // Within 45 blocks of right edge
			{
				NPC.direction = -1;
			}
		}
	}
}