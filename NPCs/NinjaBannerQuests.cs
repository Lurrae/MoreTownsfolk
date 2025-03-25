namespace MoreTownsfolk.NPCs
{
	public struct BannerQuest
	{
		/// <summary>
		/// The item IDs of all banners which can be used to fulfill this quest.
		/// </summary>
		public int[] AcceptedBannerTypes;
		/// <summary>
		/// The number of banners the player must provide to fulfill this quest.
		/// <para/> Any banner types in <see cref="AcceptedBannerTypes"/> can be used, including mixtures of different banners.
		/// </summary>
		public int Cost;
		/// <summary>
		/// The amount of time, in in-game days, that the Ninja must spend before returning to give the player their reward.
		/// <para/> Can be set to a value like <c>0.5f</c> to have the Ninja return later the same day instead of having to wait a full day.
		/// </summary>
		public float HuntTime;
		/// <summary>
		/// The item which the player receives after the Ninja finishes this hunt.
		/// </summary>
		public int RewardItem;
		/// <summary>
		/// The amount of reward items given to the player. Mostly useful for repeatable quests that award common materials like Gel.
		/// </summary>
		public int RewardQuantity;
		/// <summary>
		/// Whether or not this quest may be repeated indefinitely by the player.
		/// </summary>
		public bool Repeatable;

		public BannerQuest(int[] inputItems, int reward, int inputCost = 1, float time = 1.0f, int quantity = 1, bool infinite = false)
		{
			AcceptedBannerTypes = inputItems;
			Cost = inputCost;
			HuntTime = time;
			RewardItem = reward;
			RewardQuantity = quantity;
			Repeatable = infinite;
		}

		public BannerQuest()
		{
			AcceptedBannerTypes = [];
			Cost = 1;
			HuntTime = 1;
			RewardItem = ItemID.None;
			RewardQuantity = 1;
			Repeatable = false;
		}
	}

	public class NinjaBannerQuests
	{
		public static readonly int[] SlimeBanners =
		[
			// Colored Slimes
			ItemID.SlimeBanner,
			ItemID.GreenSlimeBanner,
			ItemID.PurpleSlimeBanner,
			ItemID.RedSlimeBanner,
			ItemID.YellowSlimeBanner,
			ItemID.BlackSlimeBanner,
			ItemID.PinkyBanner,

			// Biome Slimes
			ItemID.JungleSlimeBanner,
			ItemID.IceSlimeBanner,
			ItemID.SpikedJungleSlimeBanner,
			ItemID.SpikedIceSlimeBanner,
			ItemID.SandSlimeBanner,

			// Misc. Slimes
			ItemID.MotherSlimeBanner,

			// Hardmode Slimes
			ItemID.ToxicSludgeBanner,
			ItemID.CorruptSlimeBanner,
			ItemID.CrimslimeBanner,
			ItemID.IlluminantSlimeBanner
		];

		public static readonly List<BannerQuest> Quests = new()
		{
			// Repeatable quests
			new BannerQuest(SlimeBanners, ItemID.Gel, quantity: 100, time: 0.25f, infinite: true),

			#region One-Time Quests
			// Earlygame surface/sky quests
			new BannerQuest(SlimeBanners, ItemID.SlimeStaff, inputCost: 100),
			new BannerQuest([ItemID.ZombieBanner, ItemID.ZombieEskimoBanner, ItemID.RaincoatZombieBanner], ItemID.ZombieArm, inputCost: 5),
			new BannerQuest([ItemID.HarpyBanner], ItemID.GiantHarpyFeather, inputCost: 4),

			// Earlygame underground quests
			new BannerQuest([ItemID.BatBanner], ItemID.ChainKnife, inputCost: 5),
			new BannerQuest([ItemID.BatBanner, ItemID.IceBatBanner, ItemID.JungleBatBanner, ItemID.SporeBatBanner, ItemID.GiantBatBanner], ItemID.DepthMeter, inputCost: 4),
			new BannerQuest([ItemID.JellyfishBanner, ItemID.PinkJellyfishBanner, ItemID.GreenJellyfishBanner], ItemID.JellyfishNecklace, inputCost: 2),
			new BannerQuest([ItemID.SkeletonBanner], ItemID.BoneSword, inputCost: 4),
			new BannerQuest([ItemID.UndeadMinerBanner], ItemID.BonePickaxe),
			new BannerQuest([ItemID.WormBanner], ItemID.WhoopieCushion, inputCost: 2),

			// Earlygame ocean quests
			new BannerQuest([ItemID.SharkBanner], ItemID.DivingHelmet),

			// Pre-HM Blood Moon quests
			new BannerQuest([ItemID.BloodZombieBanner, ItemID.DripplerBanner], ItemID.SharkToothNecklace, inputCost: 2),

			// Pre-HM Underworld quests
			new BannerQuest([ItemID.DemonBanner], ItemID.DemonScythe),
			new BannerQuest([ItemID.FireImpBanner], ItemID.ObsidianRose),
			new BannerQuest([ItemID.HellbatBanner], ItemID.MagmaStone, inputCost: 3),

			// Ankh Shield component quests
			new BannerQuest([ItemID.CorruptorBanner, ItemID.FloatyGrossBanner], ItemID.Vitamins, inputCost: 2),
			new BannerQuest([ItemID.GiantBatBanner, ItemID.LightMummyBanner, ItemID.ClownBanner], ItemID.TrifoldMap, inputCost: 2),
			new BannerQuest([ItemID.MedusaBanner], ItemID.PocketMirror, inputCost: 2),
			new BannerQuest([ItemID.CrimsonAxeBanner, ItemID.CursedHammerBanner, ItemID.EnchantedSwordBanner, ItemID.CursedSkullBanner, ItemID.GiantCursedSkullBanner], ItemID.Nazar, inputCost: 2),
			new BannerQuest([ItemID.DarkMummyBanner, ItemID.BloodMummyBanner, ItemID.GreenJellyfishBanner, ItemID.PixieBanner], ItemID.Megaphone, inputCost: 2),
			new BannerQuest([ItemID.MummyBanner, ItemID.PixieBanner, ItemID.WraithBanner], ItemID.FastClock, inputCost: 2),
			new BannerQuest([ItemID.DarkMummyBanner, ItemID.BloodMummyBanner, ItemID.CorruptSlimeBanner, ItemID.CrimslimeBanner], ItemID.Blindfold, inputCost: 2),
			new BannerQuest([ItemID.HornetBanner, ItemID.MossHornetBanner, ItemID.ToxicSludgeBanner], ItemID.Bezoar, inputCost: 2),
			new BannerQuest([ItemID.ArmoredSkeletonBanner, ItemID.BlueArmoredBonesBanner, ItemID.HellArmoredBonesBanner, ItemID.RustyArmoredBonesBanner], ItemID.ArmorPolish, inputCost: 2),
			new BannerQuest([ItemID.AnglerFishBanner, ItemID.RustyArmoredBonesBanner, ItemID.WerewolfBanner], ItemID.AdhesiveBandage, inputCost: 2),

			// Early Hardmode surface quests
			new BannerQuest([ItemID.WerewolfBanner], ItemID.MoonCharm),

			// Solar Eclipse quests
			new BannerQuest([ItemID.CreatureFromTheDeepBanner], ItemID.NeptunesShell),

			// Early Hardmode Hallow quests
			new BannerQuest([ItemID.ChaosElementalBanner], ItemID.RodofDiscord, inputCost: 10),

			// Early Hardmode underground purity quests
			new BannerQuest([ItemID.SkeletonArcherBanner], ItemID.MagicQuiver, inputCost: 2),
			new BannerQuest([ItemID.SkeletonArcherBanner], ItemID.Marrow, inputCost: 4),
			new BannerQuest([ItemID.ArmoredSkeletonBanner], ItemID.BeamSword, inputCost: 3),

			// Early Hardmode Ice quests
			new BannerQuest([ItemID.IcyMermanBanner, ItemID.IceElementalBanner], ItemID.FrostStaff),
			new BannerQuest([ItemID.IcyMermanBanner, ItemID.IceElementalBanner, ItemID.ArmoredVikingBanner, ItemID.IceTortoiseBanner], ItemID.IceSickle, inputCost: 4),
			new BannerQuest([ItemID.IceTortoiseBanner], ItemID.FrozenTurtleShell, inputCost: 2),

			// Early Hardmode Desert quests
			new BannerQuest([ItemID.DesertBasiliskBanner], ItemID.AncientHorn),

			// Early Hardmode Jungle quests
			new BannerQuest([ItemID.MossHornetBanner], ItemID.TatteredBeeWing, inputCost: 3),
			new BannerQuest([ItemID.AngryTrapperBanner], ItemID.Uzi, inputCost: 4),

			// Post-Mech Underworld quests
			new BannerQuest([ItemID.RedDevilBanner], ItemID.FireFeather),

			// Post-Plantera Dungeon quests
			new BannerQuest([ItemID.BlueArmoredBonesBanner, ItemID.HellArmoredBonesBanner, ItemID.RustyArmoredBonesBanner], ItemID.Keybrand, inputCost: 6),
			new BannerQuest([ItemID.BlueArmoredBonesBanner, ItemID.HellArmoredBonesBanner, ItemID.RustyArmoredBonesBanner], ItemID.MagnetSphere, inputCost: 8),
			new BannerQuest([ItemID.BlueArmoredBonesBanner, ItemID.HellArmoredBonesBanner, ItemID.RustyArmoredBonesBanner], ItemID.BoneFeather, inputCost: 9)
			#endregion
		};
	}
}