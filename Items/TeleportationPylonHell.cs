using MoreTownsfolk.Configs;

namespace MoreTownsfolk.Items
{
	public class TeleportationPylonHell : ModItem
	{
		public override bool IsLoadingEnabled(Mod mod)
		{
			return GetInstance<ServerConfig>().ShuffleBiomePreferences;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<Tiles.HellPylonTile>());
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(gold: 10));
		}
	}
}