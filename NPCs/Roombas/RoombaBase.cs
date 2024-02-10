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

		private static int HeadIdx;
		private static Profiles.DefaultNPCProfile Profile;

		public override void Load()
		{
			HeadIdx = Mod.AddNPCHeadTexture(Type, Texture + "_Head");
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
			NPCID.Sets.NPCFramingGroup[Type] = 4;

			// Values copied from town cat's bestiary entry
			NPCID.Sets.NPCBestiaryDrawModifiers value = new() { Velocity = 0.25f };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);

			// The roomba just uses the default NPC profile since it doesn't have variants in the same way as the axolotl
			Profile = new Profiles.DefaultNPCProfile(
				Texture,
				HeadIdx/*,
				Texture + "_Party"*/);
		}

		public override void SetDefaults()
		{
			NPC.CloneDefaults(NPCID.TownCat);
			NPC.height = 20;
			NPC.width = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
			AIType = NPCID.TownCat;
			AnimationType = NPCID.TownCat; // Copies town cat's animation style exactly
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;
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
	}
}