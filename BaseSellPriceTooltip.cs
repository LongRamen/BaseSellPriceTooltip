using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace BaseSellPriceTooltip;

public class BaseSellPriceTooltip : Mod
{
	public static ModKeybind ShowHideTooltip { get; private set; } = null;

	public override void Load()
	{
		ShowHideTooltip = KeybindLoader.RegisterKeybind(this, "ShowHideTooltip", Keys.LeftShift);
	}

	public override void Unload()
	{
		ShowHideTooltip = null;
	}
}