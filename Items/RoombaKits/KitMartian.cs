using MoreTownsfolk.NPCs.Roombas;

namespace MoreTownsfolk.Items.RoombaKits
{
	public class KitMartian : KitBase
	{
		public override string RoombaName => "Martian";
		public override int RoombaType => NPCType<MartianRoomba>();
		public override ref bool RoombaBuiltBool => ref TownsfolkWorld.builtRoombaMars;

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.MartianConduitPlating, 10)
				.AddIngredient(ItemID.Nanites, 25)
				.AddCondition(Condition.NearShimmer)
				.Register();
		}
	}
}