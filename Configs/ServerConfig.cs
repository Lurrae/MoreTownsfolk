using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MoreTownsfolk.Configs
{
	public class ServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ShuffleBiomePreferences { get; set; }
	}
}