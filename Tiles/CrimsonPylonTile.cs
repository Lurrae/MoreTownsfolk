using MoreTownsfolk.Items;
using Terraria.GameContent;

namespace MoreTownsfolk.Tiles
{
	public class CrimsonPylonTile : CustomPylonType
	{
		public override int BaseItem => ItemType<TeleportationPylonCrimson>();
		public override string BaseItemKey => GetInstance<TeleportationPylonCrimson>().DisplayName.Key;

		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
		{
			return sceneData.EnoughTilesForCrimson;
		}
	}
}