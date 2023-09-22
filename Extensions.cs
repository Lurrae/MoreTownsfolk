using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;

namespace MoreTownsfolk
{
	public static class Conversions
	{
		public static int ToFrames(float seconds, int extraUpdates = 0)
		{
			return (int)(seconds * 60 * (extraUpdates + 1));
		}

		public static int ToPixels(float blocks)
		{
			return (int)(blocks * 16);
		}

		public static float ToSeconds(float frames, int extraUpdates = 0)
		{
			return frames / (60 * (extraUpdates + 1));
		}

		public static float ToBlocks(float pixels)
		{
			return pixels / 16;
		}

		public static Vector2 AngleToVector(float radians)
		{
			return new Vector2(-(float)Math.Cos(radians), (float)Math.Sin(radians));
		}
	}

	public static class ExtraItemDefaults
	{
		/// <summary>
		/// This method sets a variety of Item values common to flails.<br/>
		/// Specifically:<code>
		/// 
		/// Item.shoot = projType;
		/// Item.shootSpeed = shotVelocity;
		/// Item.useStyle = ItemUseStyleID.Shoot;
		/// 
		/// Item.noMelee = true;
		/// Item.noUseGraphic = true;
		/// Item.channel = true;</code><br/>
		/// Additionally: <br/><inheritdoc cref="DefaultToMeleeWeapon(Item, int, int, int, bool)"/>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="singleSwingTime"></param>
		/// <param name="projType"></param>
		/// <param name="shotVelocity"></param>
		/// <param name="newHeight"></param>
		/// <param name="newWidth"></param>
		/// <param name="hasAutoReuse"></param>
		public static void DefaultToFlail(this Item item, int singleSwingTime, int projType, float shotVelocity, int newHeight = 32, int newWidth = 32)
		{
			item.DefaultToMeleeWeapon(singleSwingTime, newHeight, newWidth, false);

			item.shoot = projType;
			item.shootSpeed = shotVelocity;
			item.useStyle = ItemUseStyleID.Shoot;

			item.noMelee = true;
			item.noUseGraphic = true;
			item.channel = true;
		}

		/// <summary>
		/// This method sets a variety of Item values common to boomerangs.<br/>
		/// Specifically:<code>
		/// 
		/// Item.shoot = projType;
		/// Item.shootSpeed = shotVelocity;
		/// 
		/// Item.height = 40;
		/// Item.width = 40;
		/// Item.noMelee = true;
		/// Item.noUseGraphic = true;</code><br/>
		/// Additionally: <br/><inheritdoc cref="DefaultToMeleeWeapon(Item, int, int, int, bool)"/>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="projType"></param>
		/// <param name="singleShotTime"></param>
		/// <param name="shotVelocity"></param>
		/// <param name="newHeight"></param>
		/// <param name="newWidth"></param>
		/// <param name="hasAutoReuse"></param>
		public static void DefaultToBoomerang(this Item item, int projType, int singleShotTime, float shotVelocity, int newHeight = 32, int newWidth = 32, bool hasAutoReuse = false)
		{
			item.DefaultToMeleeWeapon(singleShotTime, newHeight, newWidth, hasAutoReuse);

			item.shoot = projType;
			item.shootSpeed = shotVelocity;

			item.noMelee = true;
			item.noUseGraphic = true;
		}

		/// <summary>
		/// This method sets a variety of Item values common to swords.<br/>
		/// Specifically:<code>
		/// 
		/// Item.shoot = projType;
		/// Item.shootSpeed = shotVelocity;</code><br/>
		/// Additionally: <br/><inheritdoc cref="DefaultToMeleeWeapon(Item, int, int, int, bool)"/>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="singleSwingTime"></param>
		/// <param name="projType"></param>
		/// <param name="shotVelocity"></param>
		/// <param name="newHeight"></param>
		/// <param name="newWidth"></param>
		/// <param name="hasAutoReuse"></param>
		public static void DefaultToSword(this Item item, int singleSwingTime, int projType = 0, float shotVelocity = 0, int newHeight = 32, int newWidth = 32, bool hasAutoReuse = false)
		{
			item.DefaultToMeleeWeapon(singleSwingTime, newHeight, newWidth, hasAutoReuse);

			item.shoot = projType;
			item.shootSpeed = shotVelocity;
		}

		/// <summary>
		/// This method sets a variety of Item values common to melee weapons.<br/>
		/// Specifically:<code>
		/// 
		/// Item.useTime = singleShotTime;
		/// Item.useAnimation = singleShotTime;
		/// Item.autoReuse = hasAutoReuse;
		/// 
		/// Item.DamageType = DamageClass.Melee;
		/// Item.useStyle = <see cref="ItemUseStyleID.Swing"/>;
		/// Item.UseSound = SoundID.Item1;</code>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="singleSwingTime"></param>
		/// <param name="newHeight"></param>
		/// <param name="newWidth"></param>
		/// <param name="hasAutoReuse"></param>
		public static void DefaultToMeleeWeapon(this Item item, int singleSwingTime, int newHeight = 32, int newWidth = 32, bool hasAutoReuse = false)
		{
			item.useTime = singleSwingTime;
			item.useAnimation = singleSwingTime;
			item.autoReuse = hasAutoReuse;
			item.height = newHeight;
			item.width = newWidth;

			item.DamageType = DamageClass.Melee;
			item.useStyle = ItemUseStyleID.Swing;
			item.UseSound = SoundID.Item1;
		}

		/// <summary>
		/// This method sets a variety of Item values common to minion weapons.<br/>
		/// Specifically:<code>
		/// 
		/// Item.buffType = buffType;
		/// Item.shoot = projType;
		///
		/// Item.shootSpeed = 10;
		/// Item.UseSound = SoundID.Item44;</code><br/>
		/// Additionally: <br/><inheritdoc cref="DefaultToSummonWeapon(Item, int, int, int, int)"/>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="buffType"></param>
		/// <param name="projType"></param>
		/// <param name="singleSwingTime"></param>
		/// <param name="manaCost"></param>
		/// <param name="newHeight"></param>
		/// <param name="newWidth"></param>
		public static void DefaultToMinion(this Item item, int buffType, int projType, int singleSwingTime = 36, int manaCost = 10, int newHeight = 32, int newWidth = 32)
		{
			item.DefaultToSummonWeapon(manaCost, singleSwingTime, newHeight, newWidth);

			item.buffType = buffType;
			item.shoot = projType;

			item.shootSpeed = 10;
			item.UseSound = SoundID.Item44;
		}

		/// <summary>
		/// This method sets a variety of Item values common to sentry weapons.<br/>
		/// Specifically:<code>
		/// 
		/// Item.DefaultToSummonWeapon(manaCost, singleSwingTime, newHeight, newWidth);
		/// 
		/// Item.shoot = projType;
		/// 
		/// Item.shootSpeed = 1;
		/// Item.UseSound = SoundID.Item46;
		/// Item.sentry = true;</code><br/>
		/// Additionally: <br/><inheritdoc cref="DefaultToSummonWeapon(Item, int, int, int, int)"/>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="projType"></param>
		/// <param name="manaCost"></param>
		/// <param name="singleSwingTime"></param>
		/// <param name="newHeight"></param>
		/// <param name="newWidth"></param>
		public static void DefaultToSentry(this Item item, int projType, int manaCost = 20, int singleSwingTime = 30, int newHeight = 32, int newWidth = 32)
		{
			item.DefaultToSummonWeapon(manaCost, singleSwingTime, newHeight, newWidth);

			item.shoot = projType;

			item.shootSpeed = 1;
			item.UseSound = SoundID.Item46;
			item.sentry = true;
		}

		/// <summary>
		/// This method sets a variety of Item values common to summon weapons.<br/>
		/// Specifically:<code>
		/// 
		/// Item.mana = manaCost;
		/// Item.height = newHeight;
		/// Item.width = newWidth;
		/// Item.useTime = singleSwingTime;
		/// Item.useAnimation = singleSwingTime;
		/// 
		/// Item.DamageType = DamageClass.Summon;
		/// Item.useStyle = <see cref="ItemUseStyleID.Swing"/>;
		/// Item.noMelee = true;
		/// Item.autoReuse = false;</code>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="manaCost"></param>
		/// <param name="singleSwingTime"></param>
		/// <param name="newHeight"></param>
		/// <param name="newWidth"></param>
		public static void DefaultToSummonWeapon(this Item item, int manaCost, int singleSwingTime, int newHeight = 32, int newWidth = 32)
		{
			item.mana = manaCost;
			item.height = newHeight;
			item.width = newWidth;
			item.useTime = singleSwingTime;
			item.useAnimation = singleSwingTime;

			item.DamageType = DamageClass.Summon;
			item.useStyle = ItemUseStyleID.Swing;
			item.noMelee = true;
			item.autoReuse = false;
		}

		/// <summary>
		/// This method sets a variety of Item values common to armor pieces.<br/>
		/// Specifically:<code>
		/// 
		/// Item.defense = defense;
		/// Item.height = newHeight;
		/// Item.width = newWidth;</code>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="defense"></param>
		/// <param name="newHeight"></param>
		/// <param name="newWidth"></param>
		public static void DefaultToArmor(this Item item, int defense, int newHeight = 26, int newWidth = 26)
		{
			item.defense = defense;
			item.height = newHeight;
			item.width = newWidth;
		}

		/// <summary>
		/// This method sets a variety of Item values common to dev set pieces.<br/>
		/// Specifically:<code>
		/// 
		/// Item.rare = rarity;
		/// Item.height = newHeight;
		/// Item.width = newWidth;
		///
		/// Item.value = Item.sellPrice(gold: 5);
		/// Item.vanity = true;</code>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="rarity"></param>
		/// <param name="newHeight"></param>
		/// <param name="newWidth"></param>
		public static void DefaultToDevSet(this Item item, int rarity, int newHeight = 26, int newWidth = 26)
		{
			item.rare = rarity;
			item.height = newHeight;
			item.width = newWidth;

			item.value = Item.sellPrice(gold: 5);
			item.vanity = true;
		}

		/// <summary>
		/// A helper method that sets item rarity and item value to match that of vanilla town NPC weapon drops.
		/// Specifically:<code>
		/// 
		/// Item.rarity = ItemRarityColor.Green2;
		/// Item.value = Item.sellPrice(silver: 50);</code>
		/// </summary>
		/// <param name="item"></param>
		public static void CloneShopValues_TownNPCDrop(this Item item)
		{
			item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(silver: 50));
		}
	}

	public static class ExtraPlayerMethods
	{
		public static void LicenseOrExchangePet(this Player player, Item sItem, ref bool petBoughtFlag, int npcType, string textKeyForLicense, int netMessageData)
		{
			if (!player.ItemTimeIsZero)
			{
				return;
			}
			if (petBoughtFlag && !NPC.AnyNPCs(npcType))
			{
				return;
			}
			player.ApplyItemTime(sItem, 1, false);
			NPC.UnlockOrExchangePet(ref petBoughtFlag, npcType, textKeyForLicense, netMessageData);
		}
	}

	public static class ExtraConditions
	{
		public static Condition BestiaryCompletionPercent(float targetPercent)
		{
			string description = Language.GetTextValue("Mods.MoreTownsfolk.Conditions.BestiaryCompletionPercent", targetPercent);

			return new Condition(description, () => Main.GetBestiaryProgressReport().CompletionPercent >= targetPercent);
		}
	}

	public class ModdedVariantNPCProfile : ITownNPCProfile
	{
		private string _rootFilePath;
		private string _npcBaseName;
		private int[] _variantHeadIDs;
		private string[] _variants;
		internal Dictionary<string, Asset<Texture2D>> _variantTextures = new();

		public ModdedVariantNPCProfile(string npcFileTitleFilePath, string npcBaseName, int[] variantHeadIds, params string[] variantTextureNames)
		{
			_rootFilePath = npcFileTitleFilePath;
			_npcBaseName = npcBaseName;
			_variantHeadIDs = variantHeadIds;
			_variants = variantTextureNames;
			string[] variants = _variants;
			foreach (string text in variants)
			{
				string text2 = _rootFilePath + "_" + text;
				if (!Main.dedServ)
					_variantTextures[text2] = Request<Texture2D>(text2);
			}
		}

		public ModdedVariantNPCProfile SetPartyTextures(params string[] variantTextureNames)
		{
			foreach (string text in variantTextureNames)
			{
				string text2 = _rootFilePath + "_" + text + "_Party";
				if (!Main.dedServ)
					_variantTextures[text2] = Request<Texture2D>(text2);
			}

			return this;
		}

		public int RollVariation() => Main.rand.Next(_variants.Length);
		public string GetNameForVariant(NPC npc) => WorldGen.genRand.NextFromCollection(Language.FindAll(Lang.CreateDialogFilter("Mods.MoreTownsfolk.NPCNames." + _npcBaseName + "Names_" + _variants[npc.townNpcVariationIndex])).ToList()).Value;

		public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc)
		{
			string text = _rootFilePath + "_" + _variants[npc.townNpcVariationIndex];
			if (npc.IsABestiaryIconDummy)
				return _variantTextures[text];

			if (npc.altTexture == 1 && _variantTextures.ContainsKey(text + "_Party"))
				return _variantTextures[text + "_Party"];

			return _variantTextures[text];
		}

		public string GetTexturePath(NPC npc) => _rootFilePath + "_" + _variants[npc.townNpcVariationIndex];

		public int GetHeadTextureIndex(NPC npc) => _variantHeadIDs[npc.townNpcVariationIndex];
	}
}