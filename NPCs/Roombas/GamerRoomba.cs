
namespace MoreTownsfolk.NPCs.Roombas
{
	public class GamerRoomba : RoombaBase
	{
		public override string RoombaType => "GamerRoomba";
		public override Func<bool> RoombaKitBool => () => TownsfolkWorld.builtRoombaGamer;
	}
}