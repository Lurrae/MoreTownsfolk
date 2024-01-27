using MoreTownsfolk.TileEntities;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Map;
using Terraria.ModLoader.Default;
using Terraria.ObjectData;

namespace MoreTownsfolk.Tiles
{
	public abstract class CustomPylonType : ModPylon
	{
		public const int CrystalVerticalFrameCount = 8;

		public Asset<Texture2D> crystalTexture;
		public Asset<Texture2D> crystalHighlightTexture;
		public Asset<Texture2D> mapIcon;

		public abstract int BaseItem { get; }
		public abstract string BaseItemKey { get; }

		public sealed override void Load()
		{
			crystalTexture = Request<Texture2D>(Texture + "_Crystal");
			crystalHighlightTexture = Request<Texture2D>(Texture + "_CrystalHighlight");
			mapIcon = Request<Texture2D>(Texture + "_MapIcon");
		}

		public sealed override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;

			TEModdedPylon moddedPylon = GetInstance<ModPylonTileEntity>();
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type);

			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.PreventsSandfall[Type] = true;
			TileID.Sets.AvoidedByMeteorLanding[Type] = true;

			AddToArray(ref TileID.Sets.CountsAsPylon);

			LocalizedText pylonName = CreateMapEntryName();
			AddMapEntry(Color.White, pylonName);
		}

		public sealed override void MouseOver(int i, int j)
		{
			Main.LocalPlayer.cursorItemIconEnabled = true;
			Main.LocalPlayer.cursorItemIconID = BaseItem;
		}

		public sealed override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			GetInstance<ModPylonTileEntity>().Kill(i, j);
		}

		public sealed override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.75f;
			g = 0.00f;
			b = 0.75f;
		}

		public sealed override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -12f), Color.White * 0.1f, Color.White, 1, CrystalVerticalFrameCount);
		}

		public sealed override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale)
		{
			bool mouseOver = DefaultDrawMapIcon(ref context, mapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), drawColor, deselectedScale, selectedScale);
			DefaultMapClickHandle(mouseOver, pylonInfo, BaseItemKey, ref mouseOverText);
		}
	}
}