using Fotografia.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Extensions;

public static class QueryablePaginationExtensions
{
    public static async Task<PaginatedResponseDto<T>> ToPaginatedResponseAsync<T>(
        this IQueryable<T> query,
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        var totalItems = await query.CountAsync(cancellationToken);

        if (pagination.All)
        {
            return new PaginatedResponseDto<T>
            {
                Items = await query.ToListAsync(cancellationToken),
                Page = 1,
                PageSize = totalItems,
                TotalItems = totalItems,
                TotalPages = 1,
                HasPreviousPage = false,
                HasNextPage = false,
                All = true
            };
        }

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponseDto<T>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasPreviousPage = pagination.Page > 1,
            HasNextPage = totalPages > 0 && pagination.Page < totalPages,
            All = false
        };
    }
}
