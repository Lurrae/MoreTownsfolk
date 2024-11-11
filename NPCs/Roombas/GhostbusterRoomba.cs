namespace MoreTownsfolk.NPCs.Roombas
{
	public class GhostbusterRoomba : RoombaBase
	{
		public override string RoombaType => "GhostbusterRoomba";
		public override Func<bool> RoombaKitBool => () => TownsfolkWorld.builtRoombaGhost;
	}
}