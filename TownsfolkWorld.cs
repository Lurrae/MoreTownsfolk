using MoreTownsfolk.NPCs;
using System.IO;
using Terraria.Chat;
using Terraria.ModLoader.IO;

namespace MoreTownsfolk
{
	public class TownsfolkWorld : ModSystem
	{
		public static bool boughtAxolotl = false;
		public static bool downedEater = false;
		public static bool downedBrain = false;
		public static bool builtRoombaMaid = false;
		public static bool builtRoombaFlesh = false;
		public static bool builtRoombaGamer = false;
		public static bool builtRoombaPlant = false;
		public static bool builtRoombaGhost = false;
		public static bool builtRoombaLihz = false;
		public static bool builtRoombaMars = false;
		public static bool builtRoombaMoon = false;
		public static bool occultistSecret = false;
		public static bool savedNinja = false;
		public static int currentNinjaHunt = -1;
		//public static int currentNinjaHuntTimer = -1;
		public static float ninjaReturnTime = -1;
		public static int daysUntilReturn = -1;
		public static bool decrementedNinjaDaysToday = false;
		public static int ninjaHomeX = 0;
		public static int ninjaHomeY = 0;
		public static List<int> completedNinjaHunts = [];

		public override void PostUpdateTime()
		{
			// Only handle Ninja timer if he's on a quest and not present in the world
			if (currentNinjaHunt == -1 || NPC.AnyNPCs(NPCType<Ninja>()))
				return;

			// Once we've hit the targeted time of day for the first time today, decrement the number of days until the Ninja returns
			if (!decrementedNinjaDaysToday && Utils.GetDayTimeAs24FloatStartingFromMidnight() >= ninjaReturnTime)
			{
				daysUntilReturn--;
				decrementedNinjaDaysToday = true;

				// Ninja should return now, spawn him immediately!
				if (daysUntilReturn == 0)
				{
					int newNinja = NPC.NewNPC(Entity.GetSource_TownSpawn(), Conversions.ToPixels(ninjaHomeX), Conversions.ToPixels(ninjaHomeY), NPCType<Ninja>());
					NPC ninja = Main.npc[newNinja];

					if (Main.netMode == NetmodeID.SinglePlayer)
					{
						Main.NewText(Language.GetTextValue("Announcement.HasArrived", ninja.FullName), ChatColors.NPCArrived); // "(name) the Ninja has arrived!"
					}
					else if (Main.netMode == NetmodeID.Server)
					{
						ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasArrived", ninja.GetFullNetName()), ChatColors.NPCArrived);
					}
				}
			}
		}

		public override void ClearWorld()
		{
			boughtAxolotl = false;
			downedEater = false;
			downedBrain = false;
			occultistSecret = false;

			// Roomba bools
			builtRoombaMaid = false;
			builtRoombaFlesh = false;
			builtRoombaGamer = false;
			builtRoombaPlant = false;
			builtRoombaGhost = false;
			builtRoombaLihz = false;
			builtRoombaMars = false;
			builtRoombaMoon = false;

			// Ninja-related variables
			savedNinja = false;
			currentNinjaHunt = -1;
			//currentNinjaHuntTimer = -1;
			ninjaReturnTime = -1;
			daysUntilReturn = -1;
			decrementedNinjaDaysToday = false;
			ninjaHomeX = 0;
			ninjaHomeY = 0;
			completedNinjaHunts = [];
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (boughtAxolotl)
				tag["boughtAxolotl"] = true;

			if (downedEater)
				tag["downedEater"] = true;

			if (downedBrain)
				tag["downedBrain"] = true;

			if (occultistSecret)
				tag["occultistSecret"] = true;

			if (builtRoombaMaid)
				tag["boughtRoombaMaid"] = true;

			if (builtRoombaFlesh)
				tag["boughtRoombaFlesh"] = true;

			if (builtRoombaGamer)
				tag["boughtRoombaGamer"] = true;

			if (builtRoombaPlant)
				tag["boughtRoombaPlant"] = true;

			if (builtRoombaGhost)
				tag["boughtRoombaGhost"] = true;

			if (builtRoombaLihz)
				tag["boughtRoombaLihz"] = true;

			if (builtRoombaMars)
				tag["boughtRoombaMars"] = true;

			if (builtRoombaMoon)
				tag["boughtRoombaMoon"] = true;

			if (savedNinja)
				tag["savedNinja"] = true;

			tag["currentNinjaHunt"] = currentNinjaHunt;
			//tag["currentNinjaHuntTimer"] = currentNinjaHuntTimer;
			tag["ninjaReturnTime"] = ninjaReturnTime;
			tag["daysUntilReturn"] = daysUntilReturn;

			if (decrementedNinjaDaysToday)
				tag["decrementedNinjaDaysToday"] = true;

			tag["ninjaHomeX"] = ninjaHomeX;
			tag["ninjaHomeY"] = ninjaHomeY;
			tag["completedNinjaHunts"] = completedNinjaHunts;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			boughtAxolotl = tag.ContainsKey("boughtAxolotl");
			downedEater = tag.ContainsKey("downedEater");
			downedBrain = tag.ContainsKey("downedBrain");
			occultistSecret = tag.ContainsKey("occultistSecret");

			// Roomba bools
			builtRoombaMaid = tag.ContainsKey("boughtRoombaMaid");
			builtRoombaFlesh = tag.ContainsKey("boughtRoombaFlesh");
			builtRoombaGamer = tag.ContainsKey("boughtRoombaGamer");
			builtRoombaPlant = tag.ContainsKey("boughtRoombaPlant");
			builtRoombaGhost = tag.ContainsKey("boughtRoombaGhost");
			builtRoombaLihz = tag.ContainsKey("boughtRoombaLihz");
			builtRoombaMars = tag.ContainsKey("boughtRoombaMars");
			builtRoombaMoon = tag.ContainsKey("boughtRoombaMoon");

			// Ninja-related variables
			savedNinja = tag.ContainsKey("savedNinja");
			currentNinjaHunt = tag.GetInt("currentNinjaHunt");
			//currentNinjaHuntTimer = tag.GetInt("currentNinjaHuntTimer");
			ninjaReturnTime = tag.GetFloat("ninjaReturnTime");
			daysUntilReturn = tag.GetInt("daysUntilReturn");
			decrementedNinjaDaysToday = tag.ContainsKey("decrementedNinjaDaysToday");
			ninjaHomeX = tag.GetInt("ninjaHomeX");
			ninjaHomeY = tag.GetInt("ninjaHomeY");
			completedNinjaHunts = tag.GetList<int>("completedNinjaHunts").ToList();
		}

		public override void NetSend(BinaryWriter writer)
		{
			var flags = new BitsByte();
			flags[0] = boughtAxolotl;
			flags[1] = downedEater;
			flags[2] = downedBrain;
			flags[3] = occultistSecret;
			flags[4] = savedNinja;
			flags[5] = decrementedNinjaDaysToday;
			
			writer.Write(flags);

			// Roomba bools
			flags = new BitsByte();
			flags[0] = builtRoombaMaid;
			flags[1] = builtRoombaFlesh;
			flags[2] = builtRoombaGamer;
			flags[3] = builtRoombaPlant;
			flags[4] = builtRoombaGhost;
			flags[5] = builtRoombaLihz;
			flags[6] = builtRoombaMars;
			flags[7] = builtRoombaMoon;

			writer.Write(flags);

			writer.Write7BitEncodedInt(currentNinjaHunt);
			//writer.Write7BitEncodedInt(currentNinjaHuntTimer);
			writer.Write((double)ninjaReturnTime);
			writer.Write7BitEncodedInt(daysUntilReturn);
			writer.Write7BitEncodedInt(ninjaHomeX);
			writer.Write7BitEncodedInt(ninjaHomeY);

			// Write the length of the array so we know how many encoded ints to read later
			writer.Write7BitEncodedInt(completedNinjaHunts.Count);

			// Write the value of every int in the array
			for (int i = 0; i < completedNinjaHunts.Count; i++)
			{
				writer.Write7BitEncodedInt(completedNinjaHunts[i]);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			BitsByte flags = reader.ReadByte();
			boughtAxolotl = flags[0];
			downedEater = flags[1];
			downedBrain = flags[2];
			occultistSecret = flags[3];
			savedNinja = flags[4];
			decrementedNinjaDaysToday = flags[5];

			// Roomba bools
			flags = reader.ReadByte();
			builtRoombaMaid = flags[0];
			builtRoombaFlesh = flags[1];
			builtRoombaGamer = flags[2];
			builtRoombaPlant = flags[3];
			builtRoombaGhost = flags[4];
			builtRoombaLihz = flags[5];
			builtRoombaMars = flags[6];
			builtRoombaMoon = flags[7];

			currentNinjaHunt = reader.Read7BitEncodedInt();
			//currentNinjaHuntTimer = reader.Read7BitEncodedInt();
			ninjaReturnTime = (float)reader.ReadDouble();
			daysUntilReturn = reader.Read7BitEncodedInt();

			// Get the amount of quests completed so we know how many ints to read, then read them all
			int numCompletedQuests = reader.Read7BitEncodedInt();

			for (int i = 0; i < numCompletedQuests; i++)
			{
				completedNinjaHunts.Add(reader.Read7BitEncodedInt());
			}
		}
	}
}