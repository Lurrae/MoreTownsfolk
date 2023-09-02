using MoreTownsfolk.NPCs;
using Terraria.Audio;
using Terraria.DataStructures;

namespace MoreTownsfolk.Projectiles
{
	public enum FlailStateID
	{
		Channel,    // Currently spinning around player
		FlyOut,     // Player just stopped channeling and has launched this flail
		Return,     // Flail reached its max distance after FlyOut and is now returning
		Unused,
		FallReturn, // Player dropped the flail and then let go of LMB
		BounceBack, // Hit a tile while in FlyOut state, and bounced off the block
		Falling     // Player pressed LMB while in FlyOut, Return, or BounceBack state, and the flail has become stationary and heavily gravity-affected
	}

	public class HarvesterAttack : ModProjectile
	{
		// Main texture is copied from The Hook's projectile texture since it's the same
		public override string Texture => base.Texture.Replace("HarvesterAttack", "TheHookProj");
		public string ChainTex => Texture + "_Chain";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 5;
			ProjectileID.Sets.TrailingMode[Type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.ContinuouslyUpdateDamageStats = true;
			Projectile.hide = true;
			//Projectile.extraUpdates = 1;
		}

		// Draw the chain to the Occultist
		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 ownerCenter = Main.npc[(int)Projectile.ai[2]].Center;
			Vector2 center = Projectile.Center;
			Vector2 distVect2 = ownerCenter - center;
			float projRot = distVect2.ToRotation() - MathHelper.PiOver2;
			float distFloat = distVect2.Length();
			for (int i = 0; i < 1000; i++)
			{
				if (distFloat > 4f && !float.IsNaN(distFloat) && ChainTex != "")
				{
					distVect2.Normalize();
					distVect2 *= 8f;
					center += distVect2;
					distVect2 = ownerCenter - center;
					distFloat = distVect2.Length();
					
					Main.EntitySpriteDraw(Request<Texture2D>(ChainTex).Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y), new Rectangle(0, 0, 8, 10), lightColor, projRot, new Vector2(4, 5), 1f, SpriteEffects.None);
				}
			}

			return true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
		}

		// Set the AI fields to more readable values
		private FlailStateID CurrentState
		{
			get => (FlailStateID)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}

		private ref float ThrowDist => ref Projectile.ai[1];
		private ref float ChannelTimer => ref Projectile.localAI[1];

		public override void OnSpawn(IEntitySource source)
		{
			if (source is EntitySource_Parent src_parent && src_parent.Entity is NPC owner)
			{
				Projectile.ai[2] = owner.whoAmI;
				Projectile.originalDamage = Projectile.damage;
			}
		}

		public override void AI()
		{
			// Kill the projectile once the Harvester using this weapon stops attacking or dies/despawns
			NPC owner = Main.npc[(int)Projectile.ai[2]];

			if (!owner.active || owner.type != NPCType<Harvester>())
			{
				Projectile.Kill();
				return;
			}

			float targetDist = float.MaxValue;
			foreach (NPC target in Main.npc)
			{
				if (target.active && !target.friendly && !NPCID.Sets.ProjectileNPC[target.type])
				{
					if (targetDist > owner.Center.Distance(target.Center))
					{
						owner.target = target.whoAmI;
						targetDist = owner.Center.Distance(target.Center);
					}
				}
			}

			NPC target2 = Main.npc[owner.target];
			if (!target2.active || target2.friendly)
			{
				Projectile.Kill();
				return;
			}

			// Flail stats
			int maxThrowDist = 13;			// When initially launched, how far does this flail go before beginning to retract?
			float throwSpeed = 22f;			// How much initial velocity the flail has when launched
			float recoverDistance = 22f;	// How close to the player must this flail be while retracting before it can despawn?
			int hitCooldown = 10;			// How long does it take before an enemy can be hit by this flail again?
			int channelHitCooldown = 15;    // Same as above, but applies only while channeling

			Projectile.localNPCHitCooldown = hitCooldown;

			switch (CurrentState)
			{
				case FlailStateID.Channel:
					ChannelTimer++;

					if (ChannelTimer >= 22)
					{
						CurrentState = FlailStateID.FlyOut;
						ThrowDist = 0;
						targetDist = float.MaxValue;
						foreach (NPC target in Main.npc)
						{
							if (target.active && !target.friendly && !NPCID.Sets.ProjectileNPC[target.type])
							{
								if (targetDist > owner.Center.Distance(target.Center))
								{
									owner.target = target.whoAmI;
									targetDist = owner.Center.Distance(target.Center);
								}
							}
						}
						Projectile.velocity = owner.Center.DirectionTo(Main.npc[owner.target].position).SafeNormalize(Vector2.UnitX * owner.direction) * throwSpeed;
						Projectile.netUpdate = true;
						Projectile.Center = owner.Center;
						break;
					}

					Vector2 rotationVector = new Vector2(owner.direction).RotatedBy((float)Math.PI * 10 * (ChannelTimer / 60) * owner.direction);
					rotationVector.Y *= 0.8f;
					if (rotationVector.Y > 0f)
						rotationVector.Y *= 0.5f;

					owner.frame.Y = 21 * 64;
					Projectile.Center = owner.Center + rotationVector * 30;
					Projectile.velocity = Vector2.Zero;
					Projectile.localNPCHitCooldown = channelHitCooldown;
					break;
				case FlailStateID.FlyOut:
					bool atMaxRange = ThrowDist++ >= maxThrowDist;
					atMaxRange |= Projectile.Distance(owner.Center) >= 800;

					if (atMaxRange)
					{
						CurrentState = FlailStateID.Return;
						ThrowDist = 0;
						Projectile.netUpdate = true;
						Projectile.velocity *= 0.3f;
					}

					owner.direction = owner.Center.X < Projectile.Center.X ? 1 : -1;
					owner.frame.Y = ((Projectile.Center.Y >= owner.Center.Y - 8) ? 24 : (Projectile.Center.Y <= owner.Center.Y + 8) ? 22 : 23) * 64;
					Projectile.localNPCHitCooldown = hitCooldown;
					break;
				case FlailStateID.Return:
					Vector2 value2 = Projectile.DirectionTo(owner.Center).SafeNormalize(Vector2.Zero);
					if (Projectile.Distance(owner.Center) <= recoverDistance)
					{
						Projectile.Kill();
						return;
					}

					Projectile.velocity *= 0.98f;
					Projectile.velocity = Projectile.velocity.MoveTowards(value2 * recoverDistance, 3);
					owner.direction = owner.Center.X < Projectile.Center.X ? 1 : -1;
					owner.frame.Y = ((Projectile.Center.Y >= owner.Center.Y - 8) ? 24 : (Projectile.Center.Y <= owner.Center.Y + 8) ? 22 : 23) * 64;
					break;
				case FlailStateID.BounceBack:
					if (ThrowDist++ >= maxThrowDist + 5)
					{
						CurrentState = FlailStateID.Return;
						ThrowDist = 0;
						Projectile.netUpdate = true;
					}
					else
					{
						Projectile.localNPCHitCooldown = hitCooldown;
						Projectile.velocity.Y += 0.6f;
						Projectile.velocity.X *= 0.95f;
						owner.direction = owner.Center.X < Projectile.Center.X ? 1 : -1;
						owner.frame.Y = ((Projectile.Center.Y >= owner.Center.Y - 8) ? 24 : (Projectile.Center.Y <= owner.Center.Y + 8) ? 22 : 23) * 64;
					}
					break;
				default: // This flail should never be in the other states since it's not player-controlled, but if it is, switch to return as a failsafe
					CurrentState = FlailStateID.Return;
					ThrowDist = 0;
					Projectile.netUpdate = true;
					break;
			}

			// Set direction based on which way the flail is moving
			Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
			Projectile.spriteDirection = Projectile.direction;

			// Rotate the flail
			if (Projectile.velocity.Length() > 1)
				Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f;
			else
				Projectile.rotation = Projectile.Center.DirectionTo(owner.Center).ToRotation();

			Projectile.timeLeft = 2;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (CurrentState == FlailStateID.Channel)
				Projectile.damage = (int)(Projectile.originalDamage * 0.6f);
		}

		// When a flail hits a tile, it needs to bounce back and create some dust
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			int hitCooldown = 10;
			int extraDust = 0;
			Vector2 velocity = Projectile.velocity;

			// Bounce the projectile away from tiles on impact
			float bounceVel = 0.2f;
			if (CurrentState == FlailStateID.FlyOut || CurrentState == FlailStateID.BounceBack)
			{
				bounceVel = 0.4f;
			}
			if (oldVelocity.X != Projectile.velocity.X)
			{
				if (Math.Abs(oldVelocity.X) > 4f)
				{
					extraDust = 1;
				}
				Projectile.velocity.X = (0f - oldVelocity.X) * bounceVel;
				Projectile.localAI[0] += 1f;
			}
			if (oldVelocity.Y != Projectile.velocity.Y)
			{
				if (Math.Abs(oldVelocity.Y) > 4f)
				{
					extraDust = 1;
				}
				Projectile.velocity.Y = (0f - oldVelocity.Y) * bounceVel;
				Projectile.localAI[0] += 1f;
			}

			// While flying out, make it bounce back and create some dust
			if (CurrentState == FlailStateID.FlyOut)
			{
				CurrentState = FlailStateID.BounceBack;
				Projectile.localNPCHitCooldown = hitCooldown;
				Projectile.netUpdate = true;
				Point scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
				Point scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
				extraDust = 2;
				CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out var causedShockwaves);
				CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
				Projectile.position -= velocity; // Move the projectile backwards so it doesn't clip into the ground
			}

			// A little bit of extra dust
			if (extraDust > 0)
			{
				Projectile.netUpdate = true;
				for (int i = 0; i < extraDust; i++)
				{
					Collision.HitTiles(Projectile.position, velocity, Projectile.width, Projectile.height);
				}
				SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			}

			// After a bit, return to the NPC
			if ((CurrentState == FlailStateID.FlyOut || CurrentState == FlailStateID.Return) && Projectile.localAI[0] >= 10)
			{
				CurrentState = FlailStateID.Return;
				Projectile.tileCollide = false;
				Projectile.netUpdate = true;
			}
			return false;
		}

		private static void CreateImpactExplosion(int dustAmountMultiplier, Vector2 explosionOrigin, ref Point scanAreaStart, ref Point scanAreaEnd, int explosionRange, out bool causedShockwaves)
		{
			causedShockwaves = false;
			const int mult = 4;
			for (int i = scanAreaStart.X; i <= scanAreaEnd.X; i++)
			{
				for (int j = scanAreaStart.Y; j <= scanAreaEnd.Y; j++)
				{
					if (Vector2.Distance(explosionOrigin, new Vector2(i * 16, j * 16)) > explosionRange)
					{
						continue;
					}
					Tile tile = Framing.GetTileSafely(i, j);
					if (tile.IsActuated || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] || Main.tileFrameImportant[tile.TileType])
					{
						continue;
					}
					Tile tile2 = Framing.GetTileSafely(i, j - 1);
					if (!tile2.IsActuated && Main.tileSolid[tile2.TileType] && !Main.tileSolidTop[tile2.TileType])
					{
						continue;
					}
					int dustAmt = WorldGen.KillTile_GetTileDustAmount(fail: true, tile, i, j) * dustAmountMultiplier;
					for (int k = 0; k < dustAmt; k++)
					{
						Dust obj = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tile)];
						obj.velocity.Y -= 3f + mult * 1.5f;
						obj.velocity.Y *= Main.rand.NextFloat();
						obj.scale += mult * 0.03f;
					}
					for (int l = 0; l < dustAmt - 1; l++)
					{
						Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tile)];
						obj2.velocity.Y -= 1f + mult;
						obj2.velocity.Y *= Main.rand.NextFloat();
					}
					if (dustAmt > 0)
					{
						causedShockwaves = true;
					}
				}
			}
		}

		private void CreateImpactExplosion2_FlailTileCollision(Vector2 explosionOrigin, bool causedShockwaves, Vector2 velocityBeforeCollision)
		{
			Vector2 spinningpoint = new(7f, 0f);
			Vector2 value = new(1f, 0.7f);
			Color color = Color.White * 0.5f;
			Vector2 value2 = velocityBeforeCollision.SafeNormalize(Vector2.Zero);
			for (float num = 0f; num < 8f; num += 1f)
			{
				Vector2 value3 = spinningpoint.RotatedBy(num * ((float)Math.PI * 2f) / 8f) * value;
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
				dust.alpha = 0;
				if (!causedShockwaves)
				{
					dust.alpha = 50;
				}
				dust.color = color;
				dust.position = explosionOrigin + value3;
				dust.velocity.Y -= 0.8f;
				dust.velocity.X *= 0.8f;
				dust.fadeIn = 0.3f + Main.rand.NextFloat() * 0.4f;
				dust.scale = 0.4f;
				dust.noLight = true;
				dust.velocity += value2 * 2f;
			}
			if (!causedShockwaves)
			{
				for (float num2 = 0f; num2 < 8f; num2 += 1f)
				{
					Vector2 value4 = spinningpoint.RotatedBy(num2 * ((float)Math.PI * 2f) / 8f) * value;
					Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
					dust2.alpha = 100;
					dust2.color = color;
					dust2.position = explosionOrigin + value4;
					dust2.velocity.Y -= 1f;
					dust2.velocity.X *= 0.4f;
					dust2.fadeIn = 0.3f + Main.rand.NextFloat() * 0.4f;
					dust2.scale = 0.4f;
					dust2.noLight = true;
					dust2.velocity += value2 * 1.5f;
				}
			}
		}
	}
}