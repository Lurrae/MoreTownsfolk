using MoreTownsfolk.NPCs.Roombas;

namespace MoreTownsfolk.Items.RoombaKits
{
	public class KitPlanty : KitBase
	{
		public override string RoombaName => "Planty";
		public override int RoombaType => NPCType<PlantyRoomba>();
		public override ref bool RoombaBuiltBool => ref TownsfolkWorld.builtRoombaPlant;

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.LifeFruit)
				.AddIngredient(ItemID.ChlorophyteBar, 3)
				.AddCondition(Condition.NearShimmer)
				.Register();
		}
	}
}