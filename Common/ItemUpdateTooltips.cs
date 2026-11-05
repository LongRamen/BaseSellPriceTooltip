using BaseSellPriceTooltip.Config;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BaseSellPriceTooltip.Common;

public class ItemUpdateTooltips : GlobalItem
{
    private static readonly string PlatinumTextLocalised = Lang.inter[15].Value;
    private static readonly string GoldTextLocalised = Lang.inter[16].Value;
    private static readonly string SilverTextLocalised = Lang.inter[17].Value;
    private static readonly string CopperTextLocalised = Lang.inter[18].Value;
    private static readonly string NoValueTextLocalised = Lang.tip[51].Value;

    private static readonly string SellPriceTextLocalised = Lang.tip[49].Value;
    private static readonly string BuyPriceTextLocalised = Lang.tip[50].Value;

    private static string PlatinumColour => GetPulsingColour(220, 220, 198); // DCDCC6
    private static string GoldColour => GetPulsingColour(224, 201, 92); // E0C95C
    private static string SilverColour => GetPulsingColour(181, 192, 193); // B5C0C1
    private static string CopperColour => GetPulsingColour(246, 138, 96); // F68A60
    private static string NoValueColour => GetPulsingColour(130, 130, 130); // 828282

    private static string GetPulsingColour(byte r, byte g, byte b)
    {
        return GetPulsingColour(r, g, b, Main.mouseTextColor / 255f);
    }

    private static string GetPulsingColour(byte r, byte g, byte b, float pulse)
    {
        r = (byte)(r * pulse);
        g = (byte)(g * pulse);
        b = (byte)(b * pulse);

        return Convert.ToHexString([r, g, b]);
    }

    private static string LevelToColour(PriceLevel level)
    {
        return level switch
        {
            PriceLevel.Platinum => PlatinumColour,
            PriceLevel.Gold => GoldColour,
            PriceLevel.Silver => SilverColour,
            PriceLevel.Copper => CopperColour,
            PriceLevel.NoValue => NoValueColour,
            _ => null,
        };
    }

    private static string FormatPrice(long price, bool shortFormat, out PriceLevel priceLevel)
    {
        long platinum = price / 1000000;
        price -= platinum * 1000000;

        long gold = price / 10000;
        price -= gold * 10000;

        long silver = price / 100;
        price -= silver * 100;

        long copper = price;

        string output = "";

        priceLevel = PriceLevel.NoValue;
        if (platinum > 0)
        {
            priceLevel = PriceLevel.Platinum;
            output += shortFormat ? $"[i:PlatinumCoin][c/{PlatinumColour}:{platinum}] " : $"[c/{PlatinumColour}:{platinum} {PlatinumTextLocalised}] ";
        }
        if (gold > 0)
        {
            if (priceLevel == PriceLevel.NoValue) priceLevel = PriceLevel.Gold;
            output += shortFormat ? $"[i:GoldCoin][c/{GoldColour}:{gold}] " : $"[c/{LevelToColour(priceLevel)}:{gold} {GoldTextLocalised}] ";
        }
        if (silver > 0)
        {
            if (priceLevel == PriceLevel.NoValue) priceLevel = PriceLevel.Silver;
            output += shortFormat ? $"[i:SilverCoin][c/{SilverColour}:{silver}] " : $"[c/{LevelToColour(priceLevel)}:{silver} {SilverTextLocalised}] ";
        }
        if (copper > 0)
        {
            if (priceLevel == PriceLevel.NoValue) priceLevel = PriceLevel.Copper;
            output += shortFormat ? $"[i:CopperCoin][c/{CopperColour}:{copper}] " : $"[c/{LevelToColour(priceLevel)}:{copper} {CopperTextLocalised}] ";
        }

        if (output == "") output = $"[c/{NoValueColour}:{NoValueTextLocalised}]";

        return output.TrimEnd();
    }

    private static bool ConditionSatisfied(ShowCondition condition)
    {
        return condition switch
        {
            ShowCondition.HoldToShow => PlayerCheckKeybinds.Held,
            ShowCondition.PressToToggle => PlayerCheckKeybinds.Toggled,
            ShowCondition.NeverShow => false,
            _ => true,
        };
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.IsACoin) return;

        bool inShop = Main.LocalPlayer.talkNPC != -1;
        bool buying = item.isAShopItem;

        if (inShop && item.value == 0) return;

        // Edit vanilla tooltip line
        int vanillaLineIndex = tooltips.FindIndex(tooltipLine => tooltipLine.Mod == "Terraria" && tooltipLine.Name == "Price");
        if (vanillaLineIndex >= 0)
        {
            string vanillaTooltipLine = tooltips[vanillaLineIndex].Text;
            tooltips[vanillaLineIndex].Text = EditVanillaPriceTooltip(vanillaTooltipLine, buying);
        }

        // Check modded tooltip show condition
        if (inShop)
        {
            if (buying) { if (!ConditionSatisfied(MainConfig.Instance.ShowBaseBuyPrice)) return; }
            else { if (!ConditionSatisfied(MainConfig.Instance.ShowInNPCShops)) return; }
        }
        else { if (!ConditionSatisfied(MainConfig.Instance.ShowOutsideNPCShops)) return; }

        // Format price into tooltip line
        long price = (buying ? item.value : item.value / 5) * item.stack;
        string newTooltipLine = FormatPrice(price, MainConfig.Instance.ShortFormat, out PriceLevel priceLevel);

        // Add label if necessary
        bool doLabel = inShop || MainConfig.Instance.AlwaysShowLabel;
        if (doLabel && priceLevel != PriceLevel.NoValue)
        {
            string label = Language.GetTextValue($"Mods.BaseSellPriceTooltip.Labels.Base{(buying ? "Buy" : "Sell")}Price");
            newTooltipLine = $"[c/{LevelToColour(priceLevel)}:{label}] {newTooltipLine}";
        }

        // Insert tooltip line
        TooltipLine line = new(Mod, "BasePriceTooltip", newTooltipLine);
        if (vanillaLineIndex >= 0) tooltips.Insert(vanillaLineIndex + 1, line);
        else tooltips.Add(line);
    }

    private static string EditVanillaPriceTooltip(string vanillaTooltipLine, bool buying)
    {
        if (vanillaTooltipLine.Contains(NoValueTextLocalised)) return vanillaTooltipLine;

        string label = buying ? BuyPriceTextLocalised : SellPriceTextLocalised;
        if (!vanillaTooltipLine.Contains(label)) return vanillaTooltipLine;

        if (!MainConfig.Instance.ShortFormatVanilla)
        {
            return vanillaTooltipLine;
        }

        // Extract price
        long price = ExtractPriceFromVanillaTooltip(vanillaTooltipLine);
        if (price == 0) return vanillaTooltipLine;

        // Put into short format
        string formattedPrice = FormatPrice(price, shortFormat: true, out PriceLevel priceLevel);
        return $"[c/{LevelToColour(priceLevel)}:{label}] {formattedPrice}";
    }

    private static long ExtractPriceFromVanillaTooltip(string tooltipLine)
    {
        List<(int, int)> numberPositions = [];
        int start = -1;
        int length = 0;

        // Locate each number
        for (int i = 0; i < tooltipLine.Length; i++)
        {
            char c = tooltipLine[i];
            if (char.IsDigit(c))
            {
                if (length == 0) start = i;
                length++;
            }
            else if (length > 0)
            {
                numberPositions.Add((start, length));
                length = 0;
            }
        }
        numberPositions.Add((tooltipLine.Length, 0));

        int platinum = 0;
        int gold = 0;
        int silver = 0;
        int copper = 0;

        // Assign each number to platinum/gold/silver/copper
        for (int i = 0; i < numberPositions.Count - 1; i++)
        {
            (start, length) = numberPositions[i];
            int next = numberPositions[i + 1].Item1;

            string numberText = tooltipLine.Substring(start, length);
            int number;
            try { number = int.Parse(numberText); } catch { break; }

            string coinText = tooltipLine[(start + length)..next];

            if (coinText.Contains(PlatinumTextLocalised)) platinum = number;
            else if (coinText.Contains(GoldTextLocalised)) gold = number;
            else if (coinText.Contains(SilverTextLocalised)) silver = number;
            else if (coinText.Contains(CopperTextLocalised)) copper = number;
            else break;
        }

        return (platinum * 1000000L) + (gold * 10000L) + (silver * 100L) + copper;
    }
}
