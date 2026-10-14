namespace OnlyShop.Application.Options;

public sealed class BonusOptions
{
    public const string SectionName = "Bonus";

    public decimal AccrualPercent { get; set; } = 5m;
    public decimal MaxSpendPercent { get; set; } = 30m;
}