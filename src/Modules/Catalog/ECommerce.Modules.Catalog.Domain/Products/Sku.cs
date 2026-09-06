namespace ECommerce.Modules.Catalog.Domain.Products;

public sealed record Sku
{
    private Sku(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Sku Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new Sku(value.Trim().ToUpperInvariant());
    }
}
