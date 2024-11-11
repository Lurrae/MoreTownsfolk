using ReLogic.Content;

namespace MoreTownsfolk.NPCs.Roombas
{
	public class AncientRoomba : RoombaBase
	{
		public override string RoombaType => "AncientRoomba";
		public override Func<bool> RoombaKitBool => () => TownsfolkWorld.builtRoombaMoon;

		public override Vector2 PartyHatOffset()
		{
			return new Vector2(-8, -4);
		}

		private static Asset<Texture2D> GlowSolar;
		private static Asset<Texture2D> GlowVortex;
		private static Asset<Texture2D> GlowNebula;
		private static Asset<Texture2D> GlowStardust;

		public override void Load()
		{
			base.Load();

			RequestIfExists<Texture2D>(Texture + "_GlowSolar", out GlowSolar);
			RequestIfExists<Texture2D>(Texture + "_GlowVortex", out GlowVortex);
			RequestIfExists<Texture2D>(Texture + "_GlowNebula", out GlowNebula);
			RequestIfExists<Texture2D>(Texture + "_GlowStardust", out GlowStardust);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			// Null checks are a failsafe in case something goes catastrophically wrong
			Mod.Logger.Info(GlowSolar);
			Mod.Logger.Info(GlowVortex);
			Mod.Logger.Info(GlowNebula);
			Mod.Logger.Info(GlowStardust);
			if (GlowSolar != null && GlowVortex != null && GlowNebula != null && GlowStardust != null)
			{
				// Code adapted from vanilla's Ancient Manipulator glowmask drawing code, TileDrawing.cs lines 7722-7728
				SpriteEffects spriteEffects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				int frameToUse = Main.tileFrame[TileID.LunarCraftingStation] / 60;
				int nextFrame = (frameToUse + 1) % 4;
				float opacityTimer = (float)(Main.tileFrame[TileID.LunarCraftingStation] % 60) / 60f;
				Color value4 = Color.White;
				Vector2 offset = new(6, 16);

				Texture2D curGlow = frameToUse switch
				{
					0 => GlowNebula.Value,
					1 => GlowStardust.Value,
					2 => GlowVortex.Value,
					_ => GlowSolar.Value
				};

				Texture2D nextGlow = nextFrame switch
				{
					0 => GlowNebula.Value,
					1 => GlowStardust.Value,
					2 => GlowVortex.Value,
					_ => GlowSolar.Value
				};

				// Draw the glowmask textures with no color modification, so they appear fullbright
				Main.EntitySpriteDraw(curGlow, NPC.position - screenPos - offset, NPC.frame, Color.White * (1f - opacityTimer), NPC.rotation, Vector2.Zero, NPC.scale, spriteEffects);
				Main.EntitySpriteDraw(nextGlow, NPC.position - screenPos - offset, NPC.frame, Color.White * opacityTimer, NPC.rotation, Vector2.Zero, NPC.scale, spriteEffects);
			}

			base.PostDraw(spriteBatch, screenPos, drawColor);
		}
	}
}