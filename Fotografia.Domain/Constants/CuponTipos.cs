namespace Fotografia.Domain.Constants;

public static class CuponTipos
{
    public const string Porcentaje = "Porcentaje";
    public const string MontoFijo = "MontoFijo";

    private static readonly string[] Allowed =
    [
        Porcentaje,
        MontoFijo
    ];

    public static bool TryNormalize(string? value, out string tipo)
    {
        tipo = Porcentaje;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var match = Allowed.FirstOrDefault(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            return false;
        }

        tipo = match;
        return true;
    }
}
