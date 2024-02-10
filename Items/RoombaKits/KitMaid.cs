using MoreTownsfolk.NPCs.Roombas;

namespace MoreTownsfolk.Items.RoombaKits
{
	public class KitMaid : KitBase
	{
		public override string RoombaName => "Maid";
		public override int RoombaType => NPCType<MaidRoomba>();
		public override ref bool RoombaBuiltBool => ref TownsfolkWorld.builtRoombaMaid;
	}
}