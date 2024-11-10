using Terraria.GameContent;
using Terraria.GameContent.Bestiary;

namespace MoreTownsfolk.NPCs.Roombas
{
	[AutoloadHead]
	public abstract class RoombaBase : ModNPC
	{
		public abstract string RoombaType { get; }
		public abstract int MaxDialogues { get; }
		public abstract Func<bool> RoombaKitBool { get; }
		public abstract bool HasGlow { get; }

		private static int HeadIdx;
		private static Profiles.DefaultNPCProfile Profile;
		private static Texture2D glow;

		public override void Load()
		{
			HeadIdx = Mod.AddNPCHeadTexture(Type, Texture + "_Head");
			
			if (HasGlow)
			{
				glow = Request<Texture2D>(Texture + "_Glow").Value;
			}
		}

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.TownCat];

			// Copy the NPCID Sets of the town cat
			// Not sure how many of these are necessary
			NPCID.Sets.IsTownPet[Type] = true;
			NPCID.Sets.TownNPCBestiaryPriority.Add(Type);
			NPCID.Sets.ExtraFramesCount[Type] = 18;
			NPCID.Sets.DangerDetectRange[Type] = 250;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
			NPCID.Sets.NPCFramingGroup[Type] = 8;

			// The player needs to stand a bit closer to the roomba when they pet it
			NPCID.Sets.PlayerDistanceWhilePetting[Type] = 24;

			// Roombas cannot sit on furniture
			NPCID.Sets.CannotSitOnFurniture[Type] = true;

			// Values copied from town cat's bestiary entry
			NPCID.Sets.NPCBestiaryDrawModifiers value = new() { Velocity = 0.25f };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);

			// The roomba just uses the default NPC profile since it doesn't have variants in the same way as the axolotl
			Profile = new Profiles.DefaultNPCProfile(
				Texture,
				HeadIdx,
				Texture + "_Party");
		}

		public override void SetDefaults()
		{
			NPC.CloneDefaults(NPCID.TownCat);
			NPC.height = 20;
			NPC.width = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
			AnimationType = NPCID.TownCat; // Copies town cat's animation style exactly
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;
		}

		public override void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects)
		{
			int frame = NPC.frame.Y / NPC.frame.Height;
			int xOffset = -14;
			int yOffset = 4;

			switch (frame)
			{
				case 2:
				case 3:
				case 6:
				case 7:
				case 12:
				case 13:
				case 14:
				case 15:
					yOffset += 2;
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
				new FlavorTextBestiaryInfoElement($"Mods.MoreTownsfolk.Bestiary.{RoombaType}")
			});
		}

		// Spawns once the player has used the respective roomba kit
		public override bool CanTownNPCSpawn(int numTownNPCs)
		{
			return RoombaKitBool();
		}

		public override string GetChat()
		{
			string locKey = $"Mods.MoreTownsfolk.Dialogue.{RoombaType}.Dialogue" + Main.rand.Next(1, MaxDialogues + 1);
			return Language.GetTextValue(locKey, NPC.GivenName);
		}

		public override ITownNPCProfile TownNPCProfile()
		{
			return Profile;
		}

		public override List<string> SetNPCNameList()
		{
			List<string> names = new();

			foreach (LocalizedText text in Language.FindAll(Lang.CreateDialogFilter($"Mods.MoreTownsfolk.NPCNames.{RoombaType}")))
			{
				names.Add(text.Value);
			}

			return names;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (HasGlow)
			{
				var spriteEffects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

				// Draw the glowmask texture with no color modification, so it appears fullbright
				Main.EntitySpriteDraw(glow, NPC.position, NPC.frame, Color.White, NPC.rotation, Vector2.Zero, NPC.scale, spriteEffects);
			}
		}
	}
}