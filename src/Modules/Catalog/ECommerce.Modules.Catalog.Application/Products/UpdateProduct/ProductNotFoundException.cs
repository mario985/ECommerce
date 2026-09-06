namespace ECommerce.Modules.Catalog.Application.Products.UpdateProduct;

public sealed class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid productId)
        : base($"Product '{productId}' was not found.")
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
