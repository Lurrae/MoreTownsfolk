namespace MoreTownsfolk.NPCs.Roombas
{
	public class MartianRoomba : RoombaBase
	{
		public override string RoombaType => "MartianRoomba";
		public override Func<bool> RoombaKitBool => () => TownsfolkWorld.builtRoombaMars;

		public override Vector2 PartyHatOffset()
		{
			return new Vector2(-10, -6);
		}
	}
}