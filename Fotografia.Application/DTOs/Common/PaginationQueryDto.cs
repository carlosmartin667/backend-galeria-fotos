namespace Fotografia.Application.DTOs.Common;

public sealed class PaginationQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool All { get; set; }

    public string? Validate()
    {
        if (All)
        {
            return null;
        }

        if (Page < 1)
        {
            return "page debe ser mayor o igual a 1.";
        }

        return PageSize is 5 or 10 or 20 or 40
            ? null
            : "pageSize debe ser uno de estos valores: 5, 10, 20, 40.";
    }
}
