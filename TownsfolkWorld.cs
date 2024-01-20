using System.IO;
using Terraria.ModLoader.IO;

namespace MoreTownsfolk
{
	public class TownsfolkWorld : ModSystem
	{
		public static bool boughtAxolotl = false;
		public static bool downedEater = false;
		public static bool downedBrain = false;

		public override void ClearWorld()
		{
			boughtAxolotl = false;
			downedEater = false;
			downedBrain = false;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (boughtAxolotl)
				tag["boughtAxolotl"] = true;

			if (downedEater)
				tag["downedEater"] = true;

			if (downedBrain)
				tag["downedBrain"] = true;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			boughtAxolotl = tag.ContainsKey("boughtAxolotl");
			downedEater = tag.ContainsKey("downedEater");
			downedBrain = tag.ContainsKey("downedBrain");
		}

		public override void NetSend(BinaryWriter writer)
		{
			var flags = new BitsByte();
			flags[0] = boughtAxolotl;
			flags[1] = downedEater;
			flags[2] = downedBrain;
			
			writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader)
		{
			BitsByte flags = reader.ReadByte();
			boughtAxolotl = flags[0];
			downedEater = flags[1];
			downedBrain = flags[2];
		}
	}
}