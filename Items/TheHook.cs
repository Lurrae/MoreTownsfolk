using MoreTownsfolk.Projectiles;

namespace MoreTownsfolk.Items
{
	public class TheHook : ModItem
	{
		public override void SetStaticDefaults()
		{
			// See localization files for name and tooltip

			ItemID.Sets.ToolTipDamageMultiplier[Type] = 2;
		}

		public override void SetDefaults()
		{
			Item.DefaultToFlail(45, ProjectileType<TheHookProj>(), 12);
			Item.SetWeaponValues(32, 6.75f, 7);
			Item.CloneShopValues_TownNPCDrop();

			Item.scale = 1.1f;
		}
	}
}