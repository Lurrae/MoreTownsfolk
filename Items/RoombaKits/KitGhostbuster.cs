using MoreTownsfolk.NPCs.Roombas;

namespace MoreTownsfolk.Items.RoombaKits
{
	public class KitGhostbuster : KitBase
	{
		public override string RoombaName => "Ghostbuster";
		public override int RoombaType => NPCType<GhostbusterRoomba>();
		public override ref bool RoombaBuiltBool => ref TownsfolkWorld.builtRoombaGhost;

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.SpectreBar)
				.AddRecipeGroup("MoreTownsfolk:DungeonBricks", 5)
				.AddCondition(Condition.NearShimmer)
				.Register();
		}
	}
}