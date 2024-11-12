using MoreTownsfolk.NPCs.Roombas;

namespace MoreTownsfolk.Items.RoombaKits
{
	public class KitFleshy : KitBase
	{
		public override string RoombaName => "Fleshy";
		public override int RoombaType => NPCType<FleshyRoomba>();
		public override ref bool RoombaBuiltBool => ref TownsfolkWorld.builtRoombaFlesh;

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddRecipeGroup("MoreTownsfolk:FleshBlocks", 5)
				.AddIngredient(ItemID.SoulofLight)
				.AddIngredient(ItemID.SoulofNight)
				.AddCondition(Condition.NearShimmer)
				.Register();
		}
	}
}