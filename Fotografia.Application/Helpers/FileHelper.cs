using System.Text;
using System.Text.RegularExpressions;

namespace Fotografia.Application.Helpers;

public static partial class FileHelper
{
    private static readonly HashSet<string> SupportedImageContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public static bool IsSupportedImageContentType(string contentType)
    {
        return SupportedImageContentTypes.Contains(contentType.Trim().ToLowerInvariant());
    }

    public static string CreateStorageKey(Guid eventoId, string fileName)
    {
        var safeFileName = SanitizeFileName(fileName);
        return $"eventos/{eventoId:N}/fotos/{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}-{safeFileName}";
    }

    public static string CreateSlug(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var withoutAccents = builder.ToString().Normalize(NormalizationForm.FormC);
        var slug = NonSlugCharactersRegex().Replace(withoutAccents, "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N") : slug;
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        return UnsafeFileNameCharactersRegex().Replace(name, "-");
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.IgnoreCase)]
    private static partial Regex NonSlugCharactersRegex();

    [GeneratedRegex("[^a-zA-Z0-9._-]+")]
    private static partial Regex UnsafeFileNameCharactersRegex();
}
