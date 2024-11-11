using MoreTownsfolk.NPCs.Roombas;

namespace MoreTownsfolk.Items.RoombaKits
{
	public class KitAncient : KitBase
	{
		public override string RoombaName => "Ancient";
		public override int RoombaType => NPCType<AncientRoomba>();
		public override ref bool RoombaBuiltBool => ref TownsfolkWorld.builtRoombaMoon;

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.FragmentSolar)
				.AddIngredient(ItemID.FragmentVortex)
				.AddIngredient(ItemID.FragmentNebula)
				.AddIngredient(ItemID.FragmentStardust)
				.AddIngredient(ItemID.LunarBar)
				.AddCondition(Condition.NearShimmer)
				.Register();
		}
	}
}