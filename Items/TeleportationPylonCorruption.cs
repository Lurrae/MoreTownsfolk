namespace MoreTownsfolk.Items
{
	public class TeleportationPylonCorruption : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<Tiles.CorruptPylonTile>());
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(gold: 10));
		}
	}
}