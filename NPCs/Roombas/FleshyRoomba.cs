namespace MoreTownsfolk.NPCs.Roombas
{
	public class FleshyRoomba : RoombaBase
	{
		public override string RoombaType => "FleshyRoomba";
		public override Func<bool> RoombaKitBool => () => TownsfolkWorld.builtRoombaFlesh;
	}
}