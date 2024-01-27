namespace MoreTownsfolk
{
	public static class Extensions
	{
		public static bool IsArmor(this Item item)
		{
			return item.defense > 0 && (item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0);
		}
	}

	public static class ExtraConditions_MoreTownsfolk
	{
		public static Condition NotInSkyHeight = new("Mods.MoreTownsfolk.Conditions.NotInSkyHeight", () => Conversions.ToBlocks(Main.LocalPlayer.position.Y) >= Main.worldSurface * 0.35f);
	}
}