namespace MoreTownsfolk.NPCs.Roombas
{
	public class PlantyRoomba : RoombaBase
	{
		public override string RoombaType => "PlantyRoomba";
		public override Func<bool> RoombaKitBool => () => TownsfolkWorld.builtRoombaPlant;
	}
}