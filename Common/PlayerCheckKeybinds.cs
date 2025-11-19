using Terraria.GameInput;
using Terraria.ModLoader;

namespace BaseSellPriceTooltip.Common;

public class PlayerCheckKeybinds : ModPlayer
{
	public static bool Held { get; private set; } = false;
	public static bool Toggled { get; private set; } = false;

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (BaseSellPriceTooltip.ShowHideTooltip.JustPressed)
		{
			Toggled = !Toggled;
		}
		Held = BaseSellPriceTooltip.ShowHideTooltip.Current;
	}
}