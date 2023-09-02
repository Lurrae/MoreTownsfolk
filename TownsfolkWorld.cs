using System.IO;
using Terraria.ModLoader.IO;

namespace MoreTownsfolk
{
	public class TownsfolkWorld : ModSystem
	{
		public static bool boughtAxolotl = false;

		public override void ClearWorld()
		{
			boughtAxolotl = false;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (boughtAxolotl)
				tag["boughtAxolotl"] = true;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			boughtAxolotl = tag.ContainsKey("boughtAxolotl");
		}

		public override void NetSend(BinaryWriter writer)
		{
			var flags = new BitsByte();
			flags[0] = boughtAxolotl;
			
			writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader)
		{
			BitsByte flags = reader.ReadByte();
			boughtAxolotl = flags[0];
		}
	}
}