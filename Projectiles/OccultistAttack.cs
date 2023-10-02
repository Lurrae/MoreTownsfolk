using MoreTownsfolk.NPCs;
using System.Reflection.Metadata;
using Terraria.Audio;
using Terraria.DataStructures;
using static Humanizer.In;
using Terraria.ID;
using System;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace MoreTownsfolk.Projectiles
{
	public class OccultistAttack : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.penetrate = -1;
		}

		// Set the AI fields to more readable values
		ref float spawnTimer => ref Projectile.ai[0];
		ref float spawnedSpittle => ref Projectile.ai[1];
		int OwnerIdx
		{
			get => (int)Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}

		public override void OnSpawn(IEntitySource source)
		{
			if (source is EntitySource_Parent src_parent && src_parent.Entity is NPC owner)
			{
				OwnerIdx = owner.whoAmI;

				foreach (Projectile proj in Main.projectile.Where(p => p.type == Projectile.type && p.ai[2] == Projectile.ai[2] && p.whoAmI != Projectile.whoAmI))
				{
					proj.Kill();
				}
			}
		}

		public override void AI()
		{
			// Kill the projectile once the Occultist using this weapon dies/despawns
			NPC owner = Main.npc[OwnerIdx];

			if (!owner.active || owner.type != NPCType<Occultist>() || owner.frame.Y < 64 * 20)
			{
				Projectile.Kill();
				return;
			}

			// Lock the projectile's position to be in front of the Occultist
			Projectile.Center = owner.Center + (Vector2.UnitX * 16 * owner.direction);
			Projectile.direction = owner.direction;

			// Find the closest enemy to the Occultist
			float targetDist = float.MaxValue;
			foreach (NPC npc in Main.npc)
			{
				if (npc.active && !npc.friendly && !NPCID.Sets.ProjectileNPC[npc.type])
				{
					if (targetDist > owner.Center.Distance(npc.Center))
					{
						owner.target = npc.whoAmI;
						targetDist = owner.Center.Distance(npc.Center);
					}
				}
			}

			// If we have a valid target, try to spawn a spittle projectile
			NPC target = Main.npc[owner.target];
			if (target.active && !target.friendly)
			{
				// Calculate the spittle's trajectory
				Vector2 trajectory = CalculateTrajectory(Projectile.Center, target.Center + Vector2.UnitX * target.velocity.X * Conversions.ToPixels(6) * target.direction);

				// Spawn a spittle every 8 frames, unless we've spawned three or more
				if (spawnTimer-- <= 0 && spawnedSpittle < 3)
				{
					spawnedSpittle++;
					spawnTimer = 8;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, trajectory * 8, ProjectileType<SpittleProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: 1); // Set ai1 so that it doesn't play a death sound
				}
			}

			// Prevent this projectile from despawning (until the Occultist starts moving or dies/despawns)
			Projectile.timeLeft = 2;
		}

		const float BASE_VELOCITY = 8;
		const float GRAVITY = 0.25f;

		Vector2 CalculateTrajectory(Vector2 from, Vector2 to)
		{
			float x = from.X - to.X;
			float y = from.Y - to.Y;

			float sqrt = (float)(Math.Pow(BASE_VELOCITY, 4) - (GRAVITY * (GRAVITY * Math.Pow(x, 2) + 2 * y * Math.Pow(BASE_VELOCITY, 2))));
			float angle = (float)Math.Atan((Math.Pow(BASE_VELOCITY, 2) + Math.Sqrt(Math.Abs(sqrt))) / (GRAVITY * x));

			return new Vector2(Conversions.AngleToVector(angle).X * -Main.npc[OwnerIdx].direction, -1);
		}
	}

	public class SpittleProj : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
		}

		bool HasBounced
		{
			get => Projectile.ai[0] == 1;
			set => Projectile.ai[0] = value ? 1 : 0;
		}

		bool IsOwnedByHarvester
		{
			get => Projectile.ai[1] == 1;
			set => Projectile.ai[1] = value ? 1 : 0;
		}

		public override void AI()
		{
			// Apply gravity
			Projectile.velocity.Y += 0.25f;
			if (Projectile.velocity.Y > 16)
				Projectile.velocity.Y = 16;

			// Spawn dust
			Vector2 dustPos = new(Projectile.position.X, Projectile.position.Y + 2);
			Dust dust = Dust.NewDustDirect(dustPos, Projectile.width, Projectile.height, DustID.CorruptGibs, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 80, Scale: 1.3f);
			dust.velocity *= 0.3f;
			dust.noGravity = true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			// Spawn dust on tile collision
			for (int num403 = 0; num403 < 20; num403++)
			{
				Vector2 dustPos = new(Projectile.position.X, Projectile.position.Y + 2);
				Dust dust = Dust.NewDustDirect(dustPos, Projectile.width, Projectile.height, DustID.CorruptGibs, Alpha: 100, Scale: 2f);
				if (Main.rand.NextBool())
				{
					dust.scale *= 0.6f;
				}
				else
				{
					dust.velocity *= 1.4f;
					dust.noGravity = true;
				}
			}

			// Kill the projectile if this is the second bounce
			if (HasBounced)
				return true;

			// Invert x-velocity if we hit a wall
			if (oldVelocity.X != Projectile.velocity.X)
			{
				Projectile.position.X += Projectile.velocity.X;
				Projectile.velocity.X = -oldVelocity.X;
			}

			// Invert y-velocity if we hit the floor or ceiling
			if (oldVelocity.Y != Projectile.velocity.Y)
			{
				Projectile.position.Y += Projectile.velocity.Y;
				Projectile.velocity.Y = -oldVelocity.Y * 0.7f;
			}
			HasBounced = true;

			// Play a sound, but only if spawned by a player
			if (!IsOwnedByHarvester)
				SoundEngine.PlaySound(SoundID.NPCDeath9, Projectile.Center);

			return false;
		}

		// Play sound on death, but only if spawned by a player
		public override void OnKill(int timeLeft)
		{
			if (!IsOwnedByHarvester)
				SoundEngine.PlaySound(SoundID.NPCDeath9, Projectile.Center);
		}
	}
}