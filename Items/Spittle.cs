using MoreTownsfolk.Projectiles;
using Terraria.Audio;
using Terraria.DataStructures;

namespace MoreTownsfolk.Items
{
	public class Spittle : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToMagicWeapon(ProjectileType<SpittleProj>(), 30, 8, true);
			Item.SetWeaponValues(15, 1.5f);
			Item.CloneShopValues_TownNPCDrop();
			
			Item.useTime = 10; // One third of useAnimation, so it shoots three projectiles
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			SoundEngine.PlaySound(SoundID.Item21, position);
			
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
	}
}