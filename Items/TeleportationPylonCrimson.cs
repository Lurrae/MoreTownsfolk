namespace MoreTownsfolk.Items
{
	public class TeleportationPylonCrimson : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<Tiles.CrimsonPylonTile>());
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(gold: 10));
		}
	}
}