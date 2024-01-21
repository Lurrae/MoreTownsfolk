using System.IO;
using Terraria.ModLoader.IO;

namespace MoreTownsfolk.Items
{
	public class TownsfolkGlobalItem : GlobalItem
	{
		public override bool InstancePerEntity => true;

		public string OccultistCorruption = "None";

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (item.defense > 0 && (item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0) && OccultistCorruption != "None")
			{
				int idx = -1;
				string key = "Mods.MoreTownsfolk.OccultistCorruptions." + OccultistCorruption;

				foreach (TooltipLine line in tooltips)
				{
					if (line.Name == "ItemName")
						line.Text = Language.GetTextValue(key + ".DisplayName") + " " + line.Text;

					if (line.Name == "Defense")
					{
						if (OccultistCorruption == "Abominable" || OccultistCorruption == "Soulless" || OccultistCorruption == "Wallowing")
							line.Text = (item.defense + (int)Math.Max(Math.Floor(item.defense * 0.15f), 1)).ToString() + Lang.tip[25].Value;
						else
							line.Text = (item.defense - (int)Math.Max(Math.Floor(item.defense * 0.15f), 1)).ToString() + Lang.tip[25].Value;
					}

					if (line.Name.Contains("Tooltip") || line.Name == "Material" || line.Name == "Defense")
						idx = tooltips.IndexOf(line);
				}

				if (idx > -1)
				{
					TooltipLine line = new(Mod, "CorruptionDesc", Language.GetTextValue(key + ".Tooltip"));

					tooltips.Add(line);
				}
			}
		}

		public override void UpdateEquip(Item item, Player player)
		{
			if (OccultistCorruption == "None")
				return;

			switch (OccultistCorruption)
			{
				case "Abominable":
					player.statDefense += (int)Math.Max(Math.Floor(item.defense * 0.15f), 1);
					player.moveSpeed -= 0.02f;
					break;
				case "Deceitful":
					player.statDefense -= (int)Math.Max(Math.Floor(item.defense * 0.15f), 1);
					player.endurance += 0.02f;
					break;
				case "Fretful":
					player.statDefense -= (int)Math.Max(Math.Floor(item.defense * 0.15f), 1);
					player.moveSpeed += 0.02f;
					break;
				case "Soulless":
					player.statDefense += (int)Math.Max(Math.Floor(item.defense * 0.15f), 1);
					player.GetModPlayer<CorruptionPlayer>().healingMult -= 0.5f;
					player.GetModPlayer<CorruptionPlayer>().healingMult = Math.Max(0f, player.GetModPlayer<CorruptionPlayer>().healingMult);
					break;
				case "Wallowing":
					player.statDefense += (int)Math.Max(Math.Floor(item.defense * 0.15f), 1);
					player.GetAttackSpeed(DamageClass.Generic) -= 0.05f;
					break;
				case "Wrathful":
					player.statDefense -= (int)Math.Max(Math.Floor(item.defense * 0.15f), 1);
					player.GetDamage(DamageClass.Generic) += 0.05f;
					break;
			}
		}

		public override void SaveData(Item item, TagCompound tag)
		{
			if (OccultistCorruption != "None") // Only save non-default values
				tag["corruption"] = OccultistCorruption;
		}

		public override void LoadData(Item item, TagCompound tag)
		{
			if (tag.ContainsKey("corruption")) // Make sure the value is saved before trying to load it
				OccultistCorruption = (string)tag["corruption"];
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			writer.Write(OccultistCorruption);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			OccultistCorruption = reader.ReadString();
		}
	}
}