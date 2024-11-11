namespace MoreTownsfolk.NPCs.Roombas
{
	public class LihzahrdRoomba : RoombaBase
	{
		public override string RoombaType => "LihzahrdRoomba";
		public override Func<bool> RoombaKitBool => () => TownsfolkWorld.builtRoombaLihz;
	}
}