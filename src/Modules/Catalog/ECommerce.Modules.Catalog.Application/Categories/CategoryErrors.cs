using ECommerce.Common.Application.Errors;

namespace ECommerce.Modules.Catalog.Application.Categories;

public static class CategoryErrors
{
    public static readonly Error NotFound = new("Category.NotFound", "The requested category was not found.", ErrorType.NotFound);
    public static readonly Error DuplicateName = new("Category.DuplicateName", "The category name is already in use.", ErrorType.Conflict);
    public static readonly Error DuplicateSlug = new("Category.DuplicateSlug", "The category slug is already in use.", ErrorType.Conflict);
    public static readonly Error InvalidName = new("Category.InvalidName", "The category name is invalid.", ErrorType.Validation);
    public static readonly Error InvalidSlug = new("Category.InvalidSlug", "The category slug is invalid.", ErrorType.Validation);
    public static readonly Error AlreadyActive = new("Category.AlreadyActive", "The category is already active.", ErrorType.Conflict);
    public static readonly Error AlreadyInactive = new("Category.AlreadyInactive", "The category is already inactive.", ErrorType.Conflict);
    public static readonly Error HasActiveProducts = new("Category.HasActiveProducts", "The category has active products and cannot be deactivated.", ErrorType.Conflict);
}
