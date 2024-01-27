using Terraria.GameContent.Personalities;

namespace MoreTownsfolk.CustomShoppingBiomes
{
	public abstract class ACustomShoppingBiome : IShoppingBiome, ILoadable
	{
		public string NameKey { get; protected set; }

		public abstract bool IsInBiome(Player player);

		void ILoadable.Load(Mod mod) { }

		void ILoadable.Unload() { }
	}

	public class SkyBiome : ACustomShoppingBiome
	{
		public SkyBiome()
		{
			NameKey = "Mods.MoreTownsfolk.Biomes.Sky";
		}

		public override bool IsInBiome(Player player)
		{
			return Conversions.ToBlocks(player.position.Y) <= Main.worldSurface * 0.35f;
		}
	}

	public class HellBiome : ACustomShoppingBiome
	{
		public HellBiome()
		{
			NameKey = "Mods.MoreTownsfolk.Biomes.Hell";
		}

		public override bool IsInBiome(Player player)
		{
			return Conversions.ToBlocks(player.position.Y) >= Main.UnderworldLayer;
		}
	}
}