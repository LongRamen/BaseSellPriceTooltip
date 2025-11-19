using BaseSellPriceTooltip.Config;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BaseSellPriceTooltip.Common;

public class ItemUpdateTooltips : GlobalItem
{
	internal enum PriceLevel
	{
		Platinum,
		Gold,
		Silver,
		Copper,
		NoValue,
	}

	private static readonly string PlatinumTextLocalised = Lang.inter[15].Value;
	private static readonly string GoldTextLocalised = Lang.inter[16].Value;
	private static readonly string SilverTextLocalised = Lang.inter[17].Value;
	private static readonly string CopperTextLocalised = Lang.inter[18].Value;
	private static readonly string NoValueTextLocalised = Lang.tip[51].Value;

	private static readonly string SellPriceTextLocalised = Lang.tip[49].Value;
	private static readonly string BuyPriceTextLocalised = Lang.tip[50].Value; 

	private static readonly string PlatinumItemTag = "[i:74]";
	private static readonly string GoldItemTag = "[i:73]";
	private static readonly string SilverItemTag = "[i:72]";
	private static readonly string CopperItemTag = "[i:71]";

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
			output += shortFormat ? $"{PlatinumItemTag}[c/{PlatinumColour}:{platinum}] " : $"[c/{PlatinumColour}:{platinum} {PlatinumTextLocalised}] ";
		}
		if (gold > 0)
		{
			if (priceLevel == PriceLevel.NoValue) priceLevel = PriceLevel.Gold;
			output += shortFormat ? $"{GoldItemTag}[c/{GoldColour}:{gold}] " : $"[c/{LevelToColour(priceLevel)}:{gold} {GoldTextLocalised}] ";
		}
		if (silver > 0)
		{
			if (priceLevel == PriceLevel.NoValue) priceLevel = PriceLevel.Silver;
			output += shortFormat ? $"{SilverItemTag}[c/{SilverColour}:{silver}] " : $"[c/{LevelToColour(priceLevel)}:{silver} {SilverTextLocalised}] ";
		}
		if (copper > 0)
		{
			if (priceLevel == PriceLevel.NoValue) priceLevel = PriceLevel.Copper;
			output += shortFormat ? $"{CopperItemTag}[c/{CopperColour}:{copper}] " : $"[c/{LevelToColour(priceLevel)}:{copper} {CopperTextLocalised}] ";
		}

		if (output == "") output = $"[c/{NoValueColour}:{NoValueTextLocalised}]";

		return output.TrimEnd();
	}

	private static bool ConditionSatisfied(MainConfig.ShowCondition condition)
	{
		switch (condition)
		{
			case MainConfig.ShowCondition.HoldToShow:
				if (!PlayerCheckKeybinds.Held) return false;
				break;
			case MainConfig.ShowCondition.PressToToggle:
				if (!PlayerCheckKeybinds.Toggled) return false;
				break;
			case MainConfig.ShowCondition.NeverShow:
				return false;
		}
		return true;
	}

	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		if (item.IsACoin) return;

		bool prependLabel = Main.LocalPlayer.talkNPC != -1;
		bool buy = item.isAShopItem;

		if (prependLabel && item.value == 0) return;

		MainConfig mainConfig = ModContent.GetInstance<MainConfig>();

		int index = tooltips.FindIndex(tooltipLine => tooltipLine.Mod == "Terraria" && tooltipLine.Name == "Price");
		do
		{
			if (index < 0) break;

			string lineText_ = tooltips[index].Text;
			if (lineText_.Contains(NoValueTextLocalised)) break;

			buy = lineText_.Contains(BuyPriceTextLocalised);

			if (!mainConfig.ShortFormatVanilla)
			{
				string colour = null;
				if (lineText_.Contains(CopperTextLocalised)) colour = CopperColour;
				if (lineText_.Contains(SilverTextLocalised)) colour = SilverColour;
				if (lineText_.Contains(GoldTextLocalised)) colour = GoldColour;
				if (lineText_.Contains(PlatinumTextLocalised)) colour = PlatinumColour;

				if (colour != null) tooltips[index].Text = $"[c/{colour}:{lineText_}]";
				break;
			}

			string label_ = buy ? BuyPriceTextLocalised : SellPriceTextLocalised;

			List<(int, int)> numbers = [];
			int start = -1;
			int length = 0;

			for (int i = 0; i < lineText_.Length; i++)
			{
				char c = lineText_[i];
				if (char.IsDigit(c))
				{
					if (length == 0) start = i;
					length++;
				}
				else if (length > 0)
				{
					numbers.Add((start, length));
					length = 0;
				}
			}
			numbers.Add((lineText_.Length, 0));

			int platinum = 0;
			int gold = 0;
			int silver = 0;
			int copper = 0;

			for (int i = 0; i < numbers.Count - 1; i++)
			{
				(start, length) = numbers[i];
				string numberText = lineText_.Substring(start, length);
				int number;
				try { number = int.Parse(numberText); } catch { break; }
				(int next, _) = numbers[i + 1];
				string coinText = lineText_.Substring(start + length, next - start - length);

				if (coinText.Contains(PlatinumTextLocalised)) platinum = number;
				else if (coinText.Contains(GoldTextLocalised)) gold = number;
				else if (coinText.Contains(SilverTextLocalised)) silver = number;
				else if (coinText.Contains(CopperTextLocalised)) copper = number;
				else break;
			}

			long price = (platinum * 1000000L) + (gold * 10000L) + (silver * 100L) + copper;
			if (price == 0) break;
			string formattedPrice = FormatPrice(price, shortFormat: true, out PriceLevel priceLevel_);
			(int first, _) = numbers[0];
			lineText_ = string.Concat(lineText_.AsSpan(0, first), formattedPrice);

			int labelIndex = lineText_.IndexOf(label_);
			if (labelIndex == -1) break;
			lineText_ = lineText_.Insert(labelIndex + label_.Length, "]");
			lineText_ = lineText_.Insert(labelIndex, $"[c/{LevelToColour(priceLevel_)}:");

			tooltips[index].Text = lineText_;
			//TooltipLine line_ = new(Mod, "PriceTooltip_", lineText_);
			//tooltips.Insert(index + 1, line_); index++;
		}
		while (false);

		if (prependLabel && ((buy && !ConditionSatisfied(mainConfig.ShowBaseBuyPrice)) || (!buy && !ConditionSatisfied(mainConfig.ShowInNPCShops)))) return;
		if (!prependLabel && !ConditionSatisfied(mainConfig.ShowOutsideNPCShops)) return;

		string label = Language.GetTextValue($"Mods.BaseSellPriceTooltip.Labels.Base{(buy ? "Buy" : "Sell")}Price");

		string lineText = FormatPrice((buy ? item.value : item.value / 5) * item.stack, mainConfig.ShortFormat, out PriceLevel priceLevel);
		if (prependLabel || mainConfig.AlwaysShowLabel) lineText = $"[c/{LevelToColour(priceLevel)}:{label}] {lineText}";

		TooltipLine line = new(Mod, "BasePriceTooltip", lineText);

		if (index >= 0) tooltips.Insert(index + 1, line);
		else tooltips.Add(line);
	}
}