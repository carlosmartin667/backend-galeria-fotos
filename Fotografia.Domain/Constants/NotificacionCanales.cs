namespace Fotografia.Domain.Constants;

public static class NotificacionCanales
{
    public const string Email = "Email";
    public const string Interna = "Interna";

    private static readonly string[] Allowed =
    [
        Email,
        Interna
    ];

    public static bool TryNormalize(string? value, out string canal)
    {
        canal = Interna;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var match = Allowed.FirstOrDefault(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            return false;
        }

        canal = match;
        return true;
    }
}
