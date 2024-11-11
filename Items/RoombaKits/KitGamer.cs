using MoreTownsfolk.NPCs.Roombas;

namespace MoreTownsfolk.Items.RoombaKits
{
	public class KitGamer : KitBase
	{
		public override string RoombaName => "Gamer";
		public override int RoombaType => NPCType<GamerRoomba>();
		public override ref bool RoombaBuiltBool => ref TownsfolkWorld.builtRoombaGamer;

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.HallowedBar, 2)
				.AddIngredient(ItemID.Wire, 5)
				.AddCondition(Condition.NearShimmer)
				.Register();
		}
	}
}