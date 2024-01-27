namespace MoreTownsfolk.Items
{
	public class TeleportationPylonHell : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<Tiles.HellPylonTile>());
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(gold: 10));
		}
	}
}