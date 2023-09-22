global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using Terraria;
global using Terraria.Enums;
global using Terraria.ID;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using static Terraria.ModLoader.ModContent;
using MoreTownsfolk.NPCs;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;

namespace MoreTownsfolk
{
	public class MoreTownsfolk : Mod
	{
		private static bool PlayerTalkingToEvilFanatic = false;
		private static bool EvilFanaticBeingHoused = false;

		public override void Load()
		{
			On_WorldGen.ScoreRoom += On_WorldGen_ScoreRoom;
			On_WorldGen.GetTileTypeCountByCategory += On_WorldGen_GetTileTypeCountByCategory;
			
			On_ShopHelper.ProcessMood += On_ShopHelper_ProcessMood;
			On_ShopHelper.IsPlayerInEvilBiomes += On_ShopHelper_IsPlayerInEvilBiomes;

			On_Player.PetAnimal += On_Player_PetAnimal;
			On_Player.StopPettingAnimal += On_Player_StopPettingAnimal;
		}

		private void On_WorldGen_ScoreRoom(On_WorldGen.orig_ScoreRoom orig, int ignoreNPC, int npcTypeAskingToScoreRoom)
		{
			if (npcTypeAskingToScoreRoom == NPCType<Harvester>() || npcTypeAskingToScoreRoom == NPCType<Occultist>())
			{
				EvilFanaticBeingHoused = true;
			}

			orig(ignoreNPC, npcTypeAskingToScoreRoom);

			EvilFanaticBeingHoused = false;
		}

		private int On_WorldGen_GetTileTypeCountByCategory(On_WorldGen.orig_GetTileTypeCountByCategory orig, int[] tileTypeCounts, TileScanGroup group)
		{
			if (EvilFanaticBeingHoused && (group == TileScanGroup.Corruption || group == TileScanGroup.Crimson))
			{
				return 0;
			}

			return orig(tileTypeCounts, group);
		}

		private void On_ShopHelper_ProcessMood(On_ShopHelper.orig_ProcessMood orig, ShopHelper self, Player player, NPC npc)
		{
			if (npc.type == NPCType<Harvester>() || npc.type == NPCType<Occultist>())
			{
				PlayerTalkingToEvilFanatic = true;
			}

			orig(self, player, npc);
		}

		private bool On_ShopHelper_IsPlayerInEvilBiomes(On_ShopHelper.orig_IsPlayerInEvilBiomes orig, ShopHelper self, Player player)
		{
			if (PlayerTalkingToEvilFanatic)
				return false;

			return orig(self, player);
		}

		private void On_Player_PetAnimal(On_Player.orig_PetAnimal orig, Player self, int animalNpcIndex)
		{
			orig(self, animalNpcIndex);

			self.GetModPlayer<PettingPlayer>().pettingType = Main.npc[animalNpcIndex].type;
		}

		private void On_Player_StopPettingAnimal(On_Player.orig_StopPettingAnimal orig, Player self)
		{
			orig(self);

			self.GetModPlayer<PettingPlayer>().pettingType = -1;
		}
	}
}