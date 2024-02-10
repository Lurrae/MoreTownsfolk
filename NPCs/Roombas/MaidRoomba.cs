namespace MoreTownsfolk.NPCs.Roombas
{
	public class MaidRoomba : RoombaBase
	{
		public override string RoombaType => "MaidRoomba";
		public override int MaxDialogues => 3;
		public override Func<bool> RoombaKitBool => () => TownsfolkWorld.builtRoombaMaid;
	}
}