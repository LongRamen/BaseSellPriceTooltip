using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace BaseSellPriceTooltip.Config;

internal class MainConfig : ModConfig
{
    public static MainConfig Instance;

    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("$Mods.BaseSellPriceTooltip.Configs.MainConfig.ShowHeader")]

    [DefaultValue(ShowCondition.AlwaysShow)]
    public ShowCondition ShowOutsideNPCShops;

    [DefaultValue(ShowCondition.AlwaysShow)]
    public ShowCondition ShowInNPCShops;

    [DefaultValue(ShowCondition.AlwaysShow)]
    public ShowCondition ShowBaseBuyPrice;

    [Header("$Mods.BaseSellPriceTooltip.Configs.MainConfig.FormatHeader")]

    [DefaultValue(false)]
    public bool AlwaysShowLabel;

    [DefaultValue(false)]
    public bool ShortFormat;

    [DefaultValue(false)]
    public bool ShortFormatVanilla;
}
