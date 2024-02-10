using System.IO;
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

		public override void ClearWorld()
		{
			boughtAxolotl = false;
			downedEater = false;
			downedBrain = false;

			// Roomba bools
			builtRoombaMaid = false;
			builtRoombaFlesh = false;
			builtRoombaGamer = false;
			builtRoombaPlant = false;
			builtRoombaGhost = false;
			builtRoombaLihz = false;
			builtRoombaMars = false;
			builtRoombaMoon = false;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (boughtAxolotl)
				tag["boughtAxolotl"] = true;

			if (downedEater)
				tag["downedEater"] = true;

			if (downedBrain)
				tag["downedBrain"] = true;

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
		}

		public override void LoadWorldData(TagCompound tag)
		{
			boughtAxolotl = tag.ContainsKey("boughtAxolotl");
			downedEater = tag.ContainsKey("downedEater");
			downedBrain = tag.ContainsKey("downedBrain");

			// Roomba bools
			builtRoombaMaid = tag.ContainsKey("boughtRoombaMaid");
			builtRoombaFlesh = tag.ContainsKey("boughtRoombaFlesh");
			builtRoombaGamer = tag.ContainsKey("boughtRoombaGamer");
			builtRoombaPlant = tag.ContainsKey("boughtRoombaPlant");
			builtRoombaGhost = tag.ContainsKey("boughtRoombaGhost");
			builtRoombaLihz = tag.ContainsKey("boughtRoombaLihz");
			builtRoombaMars = tag.ContainsKey("boughtRoombaMars");
			builtRoombaMoon = tag.ContainsKey("boughtRoombaMoon");
		}

		public override void NetSend(BinaryWriter writer)
		{
			var flags = new BitsByte();
			flags[0] = boughtAxolotl;
			flags[1] = downedEater;
			flags[2] = downedBrain;
			
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
		}

		public override void NetReceive(BinaryReader reader)
		{
			BitsByte flags = reader.ReadByte();
			boughtAxolotl = flags[0];
			downedEater = flags[1];
			downedBrain = flags[2];

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
		}
	}
}