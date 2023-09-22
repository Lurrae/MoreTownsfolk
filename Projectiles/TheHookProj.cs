using TepigCore.Base.ModdedProjectile;

namespace MoreTownsfolk.Projectiles
{
	public class TheHookProj : ModFlailProj
	{
		public override string ChainTex => Texture + "_Chain";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 5;
			ProjectileID.Sets.TrailingMode[Type] = 0;
		}

		public override void FlailDefaults()
		{
			Projectile.width = 34;
			Projectile.height = 34;
			//Projectile.extraUpdates = 1;
		}

		public override void SetStats(ref int throwTime, ref float throwSpeed, ref float recoverDistance, ref float recoverDistance2, ref int hitCooldown, ref int channelHitCooldown)
		{
			throwTime = 13;
			throwSpeed = 22f;
			recoverDistance = 22f;
			recoverDistance2 = 26f;
			hitCooldown = 10;
			channelHitCooldown = 15;
		}

		public override void ExtraAI()
		{
			// Set direction based on which way the flail is moving
			Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
			Projectile.spriteDirection = Projectile.direction;

			// Rotate the flail
			if (Projectile.velocity.Length() > 1)
				Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f;
			else
				Projectile.rotation = Projectile.Center.DirectionTo(Main.player[Projectile.owner].Center).ToRotation();
		}
	}
}