namespace ECommerce.Common.Application.Pagination;

public sealed record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages);
