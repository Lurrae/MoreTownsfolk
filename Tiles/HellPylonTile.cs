using MoreTownsfolk.Items;
using Terraria.GameContent;

namespace MoreTownsfolk.Tiles
{
	public class HellPylonTile : CustomPylonType
	{
		public override int BaseItem => ItemType<TeleportationPylonHell>();
		public override string BaseItemKey => GetInstance<TeleportationPylonHell>().DisplayName.Key;

		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
		{
			return pylonInfo.PositionInTiles.Y >= Main.UnderworldLayer;
		}
	}
}