using MoreTownsfolk.Items;
using Terraria.GameContent;

namespace MoreTownsfolk.Tiles
{
	public class HellPylonTile : CustomPylonType
	{
		public override int BaseItem => ItemType<TeleportationPylonHell>();
		public override string BaseItemKey => GetInstance<TeleportationPylonHell>().DisplayName.Key;
		public override int RequiredNPCCount => 2;

		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
		{
			return pylonInfo.PositionInTiles.Y >= Main.UnderworldLayer;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Texture2D tex = Request<Texture2D>(Texture + "_Glow").Value;
			Tile tile = Main.tile[i, j];
			Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
			if (Main.drawToScreen)
				zero = Vector2.Zero;

			spriteBatch.Draw(tex, new Vector2(i * 16, j * 16) - Main.screenPosition + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
		}
	}
}