
namespace MoreTownsfolk.NPCs.Roombas
{
	public class MaidRoomba : RoombaBase
	{
		public override string RoombaType => "MaidRoomba";
		public override Func<bool> RoombaKitBool => () => TownsfolkWorld.builtRoombaMaid;

		public override Vector2 GlowOffset()
		{
			return new Vector2(4, 16);
		}
	}
}