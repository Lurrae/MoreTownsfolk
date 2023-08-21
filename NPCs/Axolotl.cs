using Terraria.GameContent;
using Terraria.GameContent.Bestiary;

namespace MoreTownPets.NPCs
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
			Main.npcFrameCount[Type] = 26;

			// Copy the NPCID Sets of the town cat
			// Not sure how many of these are necessary
			NPCID.Sets.IsTownPet[Type] = true;
			NPCID.Sets.TownNPCBestiaryPriority.Add(Type);
			NPCID.Sets.ExtraFramesCount[Type] = 18;
			NPCID.Sets.DangerDetectRange[Type] = 250;
			NPCID.Sets.ShimmerImmunity[Type] = true;
			NPCID.Sets.NPCFramingGroup[Type] = 4;

			// Values copied from town cat's bestiary entry
			NPCID.Sets.NPCBestiaryDrawModifiers value = new(0) { Velocity = 0.25f };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);

			// Create all the variant textures for the axolotl
			Profile = new ModdedVariantNPCProfile(
				Texture,
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

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("Mods.MoreTownPets.Bestiary.Axolotl")
			});
		}

		public override string GetChat()
		{
			string locKey = "Mods.MoreTownPets.Dialogue.Axolotl.Dialogue" + (Main.rand.NextBool() ? "1" : "2");
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

				if (frame.Y < 22 * 40)
				{
					frame.Y = 22 * 40;
				}

				if (!Main.gamePaused && !Main.gameInactive) // Pause animation while game is paused or inactive
					swimTimer++;
				
				if (swimTimer > 10)
				{
					swimTimer = 0;
					frame.Y += 40;

					if (frame.Y > 25 * 40)
					{
						frame.Y = 22 * 40;
					}
				}

				spriteBatch.Draw(Profile.GetTextureNPCShouldUse(NPC).Value, rect, frame, drawColor, NPC.rotation, origin, spriteEffects, 0);
				return false;
			}

			return base.PreDraw(spriteBatch, screenPos, drawColor);
		}

		// Stop vertical movement in water
		int timer = 0;
		public override void PostAI()
		{
			if (NPC.wet)
			{
				float targetY = (float)Math.Sin(timer/24);
				timer++;

				NPC.velocity = NPC.velocity.MoveTowards(new Vector2(4.5f * NPC.direction, targetY * 0.5f), 0.5f);
				
				// Drowning immunity
				NPC.breath = NPC.breathMax;
			}

			// If near either edge of the world, turn around
			// Without this, the axolotls would be able to swim off into the sunset, never to be seen again
			// ...Until the following morning when they respawn, of course
			if (NPC.direction == -1 && NPC.position.X <= 0 + Conversions.ToPixels(45)) // Within 45 blocks of left edge
			{
				NPC.direction = 1;
			}
			else if (NPC.direction == 1 && NPC.position.X >= Main.maxTilesX - Conversions.ToPixels(45)) // Within 45 blocks of right edge
			{
				NPC.direction = -1;
			}
		}
	}
}