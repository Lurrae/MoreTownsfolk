using MoreTownsfolk.NPCs;
using Terraria.GameContent.Achievements;

namespace MoreTownsfolk
{
	// Handles petting custom town pets
	public class PettingPlayer : ModPlayer
	{
		public int pettingType = -1;

		public override void PostUpdate()
		{
			if (Player.isPettingAnimal && pettingType != -1)
			{
				int counter = Player.miscCounter % 14 / 7;
				Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.ThreeQuarters;
				if (counter == 1)
					stretch = Player.CompositeArmStretchAmount.Full;

				if (pettingType == NPCType<Axolotl>())
				{
					Player.SetCompositeArmBack(true, stretch, -MathHelper.TwoPi * 0.1f * Player.direction);
				}
			}
		}
	}
}