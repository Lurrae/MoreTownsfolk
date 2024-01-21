namespace MoreTownsfolk
{
	public static class Extensions
	{
		public static bool IsArmor(this Item item)
		{
			return item.defense > 0 && (item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0);
		}
	}
}