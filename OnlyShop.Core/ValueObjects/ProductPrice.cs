using OnlyShop.Core.Common;
using OnlyShop.Core.Enums;

namespace OnlyShop.Core.ValueObjects;

public readonly record struct ProductPrice
{
    public ProductPrice(decimal byn, decimal rub)
    {
        if (byn <= 0 || rub <= 0)
        {
            throw new DomainException("Price must be positive in both currencies");
        }

        Byn = byn;
        Rub = rub;
    }

    public decimal Byn { get; }
    public decimal Rub { get; }

    public decimal In(Currency currency) => currency switch
    {
        Currency.Byn => Byn,
        Currency.Rub => Rub,
        _ => throw new ArgumentOutOfRangeException(nameof(currency), currency, null)
    };
}