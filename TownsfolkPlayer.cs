using MoreTownsfolk.NPCs;
using MoreTownsfolk.NPCs.Roombas;

namespace MoreTownsfolk
{
	// Handles petting custom town pets
	public class PettingPlayer : ModPlayer
	{
		public int pettingType = -1;

		private static readonly List<int> Roombas = new()
		{
			NPCType<MaidRoomba>()
		};

		public override void PostUpdate()
		{
			if (Player.isPettingAnimal && pettingType != -1)
			{
				int counter = Player.miscCounter % 14 / 7;
				Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.ThreeQuarters;
				if (counter == 1)
					stretch = Player.CompositeArmStretchAmount.Full;

				if (pettingType == NPCType<Axolotl>() || Roombas.Contains(pettingType))
				{
					Player.SetCompositeArmBack(true, stretch, -MathHelper.TwoPi * 0.1f * Player.direction);
				}
			}
		}
	}

	// Handles custom effects from the Occultist's Corruptions
	public class CorruptionPlayer : ModPlayer
	{
		public float healingMult = 1f;

		public override void ResetEffects()
		{
			healingMult = 1f;
		}

		public override void UpdateLifeRegen()
		{
			if (Player.lifeRegen > 0)
				Player.lifeRegen = Math.Max(0, (int)Math.Round(Player.lifeRegen * healingMult));
		}

		public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
		{
			if (healingMult != 1f)
				healValue = (int)Math.Round(healValue * healingMult);
		}
	}
}