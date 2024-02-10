using MoreTownsfolk.NPCs.Roombas;

namespace MoreTownsfolk.Items.RoombaKits
{
	public abstract class KitBase : ModItem
	{
		public abstract string RoombaName { get; }
		public abstract int RoombaType { get; }
		public abstract ref bool RoombaBuiltBool { get; }

		public override LocalizedText Tooltip => Language.GetText("Mods.MoreTownsfolk.Common.RoombaKitTip").WithFormatArgs(RoombaName);

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.LicenseCat);
			Item.UseSound = SoundID.Item37;
		}

		public override bool CanUseItem(Player player)
		{
			return !NPC.AnyNPCs(NPCType<MaidRoomba>());
		}

		public override bool? UseItem(Player player)
		{
			if (player.itemAnimation < Item.useAnimation)
				return base.UseItem(player);

			NPC roomba = NPC.NewNPCDirect(player.GetSource_ItemUse(Item), player.Center, NPCType<MaidRoomba>());
			roomba.netUpdate = true;
			Main.NewText(Language.GetTextValue($"Mods.MoreTownsfolk.Common.{RoombaName}KitUsed", roomba.GivenName));
			RoombaBuiltBool = true;

			return base.UseItem(player);
		}
	}
}