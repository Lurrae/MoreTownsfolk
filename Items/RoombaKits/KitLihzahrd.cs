using MoreTownsfolk.NPCs.Roombas;

namespace MoreTownsfolk.Items.RoombaKits
{
	public class KitLihzahrd : KitBase
	{
		public override string RoombaName => "Sunny";
		public override int RoombaType => NPCType<LihzahrdRoomba>();
		public override ref bool RoombaBuiltBool => ref TownsfolkWorld.builtRoombaLihz;

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.LihzahrdPowerCell)
				.AddIngredient(ItemID.LunarTabletFragment, 3)
				.AddIngredient(ItemID.BeetleHusk)
				.AddCondition(Condition.NearShimmer)
				.Register();
		}
	}
}