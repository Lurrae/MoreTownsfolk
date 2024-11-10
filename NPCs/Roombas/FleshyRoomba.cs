namespace MoreTownsfolk.NPCs.Roombas
{
	public class FleshyRoomba : RoombaBase
	{
		public override string RoombaType => "FleshyRoomba";
		public override int MaxDialogues => 4;
		public override Func<bool> RoombaKitBool => () => TownsfolkWorld.builtRoombaFlesh;
		public override bool HasGlow => false;
	}
}