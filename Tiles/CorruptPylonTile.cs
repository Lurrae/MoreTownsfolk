using MoreTownsfolk.Items;
using Terraria.GameContent;

namespace MoreTownsfolk.Tiles
{
	public class CorruptPylonTile : CustomPylonType
	{
		public override int BaseItem => ItemType<TeleportationPylonCorruption>();
		public override string BaseItemKey => GetInstance<TeleportationPylonCorruption>().DisplayName.Key;

		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
		{
			return sceneData.EnoughTilesForCorruption;
		}
	}
}