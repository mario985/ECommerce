namespace ECommerce.Common.Application.Pagination;

public sealed record PaginationParameters(int Page = 1, int PageSize = 20)
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;

    public bool IsValid => Page >= 1 && PageSize is >= 1 and <= MaximumPageSize;
}
