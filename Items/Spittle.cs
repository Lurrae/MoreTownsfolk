using MoreTownsfolk.NPCs;
using MoreTownsfolk.Projectiles;
using rail;
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

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			velocity = (new Vector2(0.3f, -1)).RotatedBy(MathHelper.ToRadians(position.Distance(Main.MouseWorld) * 0.06f)) * Item.shootSpeed;
			velocity.X *= player.direction;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			SoundEngine.PlaySound(SoundID.Item21, position);
			
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
	}
}