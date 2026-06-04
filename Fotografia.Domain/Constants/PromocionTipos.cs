namespace Fotografia.Domain.Constants;

public static class PromocionTipos
{
    public const string General = "General";
    public const string Servicio = "Servicio";
    public const string Evento = "Evento";
    public const string Temporada = "Temporada";
    public const string Cupon = "Cupon";

    private static readonly string[] Allowed =
    [
        General,
        Servicio,
        Evento,
        Temporada,
        Cupon
    ];

    public static bool TryNormalize(string? value, out string tipo)
    {
        tipo = General;
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
