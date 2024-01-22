using MoreTownsfolk.Items;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace MoreTownsfolk
{
	public class UIDrawer : ModSystem
	{
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseIdx = layers.FindIndex((GameInterfaceLayer l) => l.Name == "Vanilla: Mouse Text");

			if (mouseIdx != -1)
			{
				layers.Insert(mouseIdx, new LegacyGameInterfaceLayer("Occult UI", delegate ()
				{
					OccultUI.Draw(Main.spriteBatch);
					return true;
				}, InterfaceScaleType.None));
			}
		}
	}

	public class OccultUI
	{
		public static int NPCIndex = -1;
		public static bool CurrentlyViewing = false;
		public static Item CurrentlyHeldItem = new();
		public static float CorruptButtonClickCountdown = 0f;

		public static readonly List<string> Corruptions = new()
		{
			"Soulless",
			"Wallowing",
			"Abominable",
			"Wrathful",
			"Deceitful",
			"Fretful"
		};

		public static Rectangle MouseScreenArea
		{
			get
			{
				return Utils.CenteredRectangle(Main.MouseScreen, Vector2.One * 2f);
			}
		}

		public static bool InRangeOfNPC()
		{
			return Main.npc.IndexInRange(NPCIndex) && Main.npc[NPCIndex].active && Utils.CenteredRectangle(Main.LocalPlayer.Center, new Vector2(Player.tileRangeX * 3f, Player.tileRangeY * 2f) * 16f).Intersects(Main.npc[NPCIndex].Hitbox);
		}

		public static void Draw(SpriteBatch spriteBatch)
		{
			Vector2 topLeft = new(68f, 320f);

			if (CorruptButtonClickCountdown > 0f)
				CorruptButtonClickCountdown--;

			if (!CurrentlyViewing)
			{
				if (!CurrentlyHeldItem.IsAir)
				{
					Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Misc(CurrentlyHeldItem.Name), CurrentlyHeldItem, CurrentlyHeldItem.stack);
					CurrentlyHeldItem.TurnToAir();
				}

				NPCIndex = -1;
				return;
			}

			if (Main.LocalPlayer.chest != -1 || Main.LocalPlayer.sign != -1 || Main.LocalPlayer.talkNPC == -1 || !Main.playerInventory || !InRangeOfNPC() || Main.InGuideCraftMenu)
			{
				CurrentlyViewing = false;
				Main.LocalPlayer.dropItemCheck();
				Recipe.FindRecipes();
				return;
			}

			Main.playerInventory = true;
			Main.craftingHide = true;
			Main.npcChatText = string.Empty;

			Vector2 bgScale = Vector2.One * Main.UIScale;

			Point costDrawPositionTopLeft = (topLeft + new Vector2(60f, -50f) * bgScale).ToPoint();
			int cost = DrawCorruptionCost(spriteBatch, costDrawPositionTopLeft);

			Vector2 itemSlotDrawPosition = topLeft;
			Vector2 reforgeIconDrawPosition = topLeft + new Vector2(60f, 30f) * bgScale;
			DrawItemIcon(spriteBatch, itemSlotDrawPosition, reforgeIconDrawPosition, bgScale, out bool isHoveringOverItemIcon, out bool isHoveringOverReforgeIcon);

			if (isHoveringOverItemIcon)
			{
				InteractWithItemSlot();
				Main.LocalPlayer.mouseInterface = false;
				Main.blockMouse = true;
			}

			if (isHoveringOverReforgeIcon)
			{
				Main.instance.MouseText(Language.GetTextValue("Mods.MoreTownsfolk.Common.CorruptButton"));
				Main.LocalPlayer.mouseInterface = false;
				Main.blockMouse = true;

				if (Main.mouseLeft && Main.mouseLeftRelease)
				{
					InteractWithCorruptIcon(cost);
					CorruptButtonClickCountdown = 15f;
				}
			}
		}

		public static int DrawCorruptionCost(SpriteBatch spriteBatch, Point costDrawPositionTopLeft)
		{
			if (CurrentlyHeldItem.IsAir)
			{
				string text = Language.GetTextValue("Mods.MoreTownsfolk.Common.PlaceArmorHere");
				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, text, costDrawPositionTopLeft.X, costDrawPositionTopLeft.Y + 45f * Main.UIScale, Color.White * (Main.mouseTextColor / 255f), Color.Black, Vector2.Zero, Main.UIScale);
				return 0;
			}

			int cost = CurrentlyHeldItem.value;
			if (Main.LocalPlayer.discountAvailable)
				cost = (int)Math.Round(cost * 0.8f);
			cost = (int)Math.Round(cost * Main.LocalPlayer.currentShoppingSettings.PriceAdjustment);
			cost = (int)Math.Round(cost * 1.5f);

			string costText = Language.GetTextValue("Mods.MoreTownsfolk.Common.Cost");
			Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, costText, costDrawPositionTopLeft.X, costDrawPositionTopLeft.Y + 45f * Main.UIScale, Color.White * (Main.mouseTextColor / 255f), Color.Black, Vector2.Zero, Main.UIScale);
			costDrawPositionTopLeft.X += (int)((FontAssets.MouseText.Value.MeasureString(costText).X * 0.5f + 12f) * Main.UIScale);
			int[] coinsArray = Utils.CoinsSplit(cost);
			float y = costDrawPositionTopLeft.Y + 54f * Main.UIScale;

			for (int i = 0; i < 4; i++)
			{
				Vector2 drawPosition = new(costDrawPositionTopLeft.X + (ChatManager.GetStringSize(FontAssets.MouseText.Value, costText, Vector2.One, -1f).X + ((24 * i) - 24f)) * Main.UIScale, y);
				spriteBatch.Draw(TextureAssets.Item[ItemID.PlatinumCoin - i].Value, drawPosition, null, Color.White, 0f, TextureAssets.Item[ItemID.PlatinumCoin - i].Size() * 0.5f, Main.UIScale, SpriteEffects.None, 0);
				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, coinsArray[3 - i].ToString(), drawPosition.X - 11f, drawPosition.Y, Color.White, Color.Black, new Vector2(0.3f), 0.75f * Main.UIScale);
			}
			
			return cost;
		}

		public static void DrawItemIcon(SpriteBatch spriteBatch, Vector2 itemSlotDrawPosition, Vector2 reforgeIconDrawPosition, Vector2 scale, out bool isHoveringOverItemIcon, out bool isHoveringOverReforgeIcon)
		{
			isHoveringOverReforgeIcon = false;

			Texture2D itemSlotTexture = TextureAssets.InventoryBack9.Value;
			Texture2D reforgeIconTexture = TextureAssets.Reforge[0].Value;
			Rectangle reforgeIconArea = new((int)reforgeIconDrawPosition.X, (int)reforgeIconDrawPosition.Y, (int)(reforgeIconTexture.Width * scale.X), (int)(itemSlotTexture.Height * scale.Y));

			if (MouseScreenArea.Intersects(reforgeIconArea))
			{
				reforgeIconTexture = TextureAssets.Reforge[1].Value;
				isHoveringOverReforgeIcon = true;
			}

			if (CorruptButtonClickCountdown > 0f)
			{
				reforgeIconTexture = TextureAssets.Reforge[1].Value;
			}

			isHoveringOverItemIcon = MouseScreenArea.Intersects(new Rectangle((int)itemSlotDrawPosition.X, (int)itemSlotDrawPosition.Y, (int)(itemSlotTexture.Width * scale.X), (int)(itemSlotTexture.Height * scale.Y)));
			spriteBatch.Draw(itemSlotTexture, itemSlotDrawPosition, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);

			if (!CurrentlyHeldItem.IsAir)
				AttemptToDrawItemInIcon(spriteBatch, itemSlotDrawPosition);

			spriteBatch.Draw(reforgeIconTexture, reforgeIconDrawPosition, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
		}

		public static void AttemptToDrawItemInIcon(SpriteBatch spriteBatch, Vector2 itemSlotDrawPosition)
		{
			float scale = Main.inventoryScale;
			Texture2D itemTexture = TextureAssets.Item[CurrentlyHeldItem.type].Value;
			Rectangle itemFrame = itemTexture.Frame();
			if (Main.itemAnimations[CurrentlyHeldItem.type] != null)
			{
				itemFrame = Main.itemAnimations[CurrentlyHeldItem.type].GetFrame(itemTexture);
			}

			float baseScale = Main.UIScale;
			float itemScale = 1f;
			Color _ = Color.White;
			ItemSlot.GetItemLight(ref _, ref baseScale, CurrentlyHeldItem);

			if (itemFrame.Width > 36 || itemFrame.Height > 36)
			{
				itemScale = 36f / MathHelper.Max(itemFrame.Width, itemFrame.Height);
			}

			itemScale *= scale * baseScale * 1.5f;
			itemSlotDrawPosition += Vector2.One * 24f * baseScale;

			spriteBatch.Draw(itemTexture, itemSlotDrawPosition, new Rectangle?(itemFrame), CurrentlyHeldItem.GetAlpha(Color.White), 0, itemFrame.Size() * 0.5f, itemScale, SpriteEffects.None, 0);
			spriteBatch.Draw(itemTexture, itemSlotDrawPosition, new Rectangle?(itemFrame), CurrentlyHeldItem.GetColor(Color.White), 0, itemFrame.Size() * 0.5f, itemScale, SpriteEffects.None, 0);
		}

		public static void InteractWithItemSlot()
		{
			if (!CurrentlyHeldItem.IsAir)
			{
				Main.HoverItem = CurrentlyHeldItem.Clone();
				Main.instance.MouseTextHackZoom(string.Empty);
			}

			if (Main.mouseLeft && Main.mouseLeftRelease && (Main.mouseItem.IsArmor() || Main.mouseItem.IsAir))
			{
				Utils.Swap(ref Main.mouseItem, ref CurrentlyHeldItem);
				SoundEngine.PlaySound(SoundID.Grab, Main.LocalPlayer.Center);
			}
		}

		public static void InteractWithCorruptIcon(int cost)
		{
			if (CurrentlyHeldItem.IsAir)
				return;

			if (cost <= 0 || !Main.LocalPlayer.CanAfford(cost))
				return;

			int oldPrefix = CurrentlyHeldItem.prefix;
			CurrentlyHeldItem.SetDefaults(CurrentlyHeldItem.type);
			CurrentlyHeldItem.Prefix(oldPrefix);
			CurrentlyHeldItem = CurrentlyHeldItem.Clone();

			CurrentlyHeldItem.GetGlobalItem<TownsfolkGlobalItem>().OccultistCorruption = Corruptions[Main.rand.Next(Corruptions.Count)];
			Main.LocalPlayer.BuyItem(cost);
			SoundEngine.PlaySound(SoundID.AbigailUpgrade, Main.LocalPlayer.Center);

			string originalName = CurrentlyHeldItem.Name;
			CurrentlyHeldItem.SetNameOverride(CurrentlyHeldItem.GetGlobalItem<TownsfolkGlobalItem>().OccultistCorruption + " " + originalName);
			CurrentlyHeldItem.position.X = Main.LocalPlayer.position.X + (Main.LocalPlayer.width / 2) - CurrentlyHeldItem.width / 2;
			CurrentlyHeldItem.position.Y = Main.LocalPlayer.position.Y + (Main.LocalPlayer.height / 2) - CurrentlyHeldItem.height / 2;
			PopupText.NewText(PopupTextContext.ItemReforge, CurrentlyHeldItem, CurrentlyHeldItem.stack, true);
			CurrentlyHeldItem.SetNameOverride(originalName);
		}
	}
}