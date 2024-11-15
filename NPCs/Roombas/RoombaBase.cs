using ReLogic.Content;
using System.Runtime.CompilerServices;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;

namespace MoreTownsfolk.NPCs.Roombas
{
	[AutoloadHead]
	public abstract class RoombaBase : ModNPC
	{
		public abstract string RoombaType { get; }
		public abstract Func<bool> RoombaKitBool { get; }
		public virtual Vector2 GlowOffset() { return new Vector2(8, 16); }
		public virtual Vector2 PartyHatOffset() { return new Vector2(-14, 4); }

		private static readonly Dictionary<string, int> HeadIdxs = new();
		private static readonly Dictionary<string, Profiles.DefaultNPCProfile> Profiles = new();
		private static readonly Dictionary<string, Asset<Texture2D>> Glows = new();
		private static readonly Dictionary<string, Asset<Texture2D>> PartyGlows = new();

		public override void Load()
		{
			HeadIdxs.Add(RoombaType, Mod.AddNPCHeadTexture(Type, Texture + "_Head"));

			RequestIfExists<Texture2D>(Texture + "_Glow", out Asset<Texture2D> Glow);
			Glows.Add(RoombaType, Glow);
			RequestIfExists<Texture2D>(Texture + "_PartyGlow", out Asset<Texture2D> PartyGlow);
			PartyGlows.Add(RoombaType, PartyGlow);
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
			// Not all roombas have a party texture, though, so that needs to be accounted for
			if (HasAsset(Texture + "_Party"))
			{
				Profiles.Add(RoombaType, new Profiles.DefaultNPCProfile(Texture, HeadIdxs[RoombaType], Texture + "_Party"));
			}
			else
			{
				Profiles.Add(RoombaType, new Profiles.DefaultNPCProfile(Texture, HeadIdxs[RoombaType]));
			}
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
			float xOffset = PartyHatOffset().X;
			float yOffset = PartyHatOffset().Y;

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
			string locKey = $"Mods.MoreTownsfolk.Dialogue.{RoombaType}.Dialogue" + Main.rand.Next(1, 4);
			return Language.GetTextValue(locKey, NPC.GivenName);
		}

		public override ITownNPCProfile TownNPCProfile()
		{
			return Profiles[RoombaType];
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

		[UnsafeAccessor(UnsafeAccessorKind.Method)]
		private static extern void DrawNPCExtras(Main self, NPC n, bool beforeDraw, float addHeight, float addY, Color npcColor, Vector2 halfSize, SpriteEffects npcSpriteEffect, Vector2 screenPosition);

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			SpriteEffects spriteEffects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			if (Glows[RoombaType] != null)
			{
				Texture2D glowSprite = Glows[RoombaType].Value;
				Vector2 offset = GlowOffset();

				if (PartyGlows[RoombaType] != null && BirthdayParty.PartyIsUp)
				{
					glowSprite = PartyGlows[RoombaType].Value;
				}

				// Draw the glowmask texture with no color modification, so it appears fullbright
				Main.EntitySpriteDraw(glowSprite, NPC.position - screenPos - offset + new Vector2(0, NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, Vector2.Zero, NPC.scale, spriteEffects);
			}

			// Draw party hat after the glowmask, so it's layered above it
			DrawNPCExtras(Main.instance, NPC, false, 0f, 0f, drawColor, Vector2.Zero, spriteEffects, screenPos);
		}
	}
}