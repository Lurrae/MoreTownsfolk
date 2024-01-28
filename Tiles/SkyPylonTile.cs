using MoreTownsfolk.Items;
using Terraria.GameContent;

namespace MoreTownsfolk.Tiles
{
	public class SkyPylonTile : CustomPylonType
	{
		public override int BaseItem => ItemType<TeleportationPylonSky>();
		public override string BaseItemKey => GetInstance<TeleportationPylonSky>().DisplayName.Key;
		public override int RequiredNPCCount => 2;

		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
		{
			return pylonInfo.PositionInTiles.Y <= Main.worldSurface * 0.35f;
		}
	}
}