namespace Fotografia.Domain.Constants;

public static class AgendaItemTipos
{
    public const string Evento = "Evento";
    public const string SesionPrivada = "SesionPrivada";
    public const string Reunion = "Reunion";
    public const string Bloqueo = "Bloqueo";
    public const string Presupuesto = "Presupuesto";
    public const string Otro = "Otro";

    private static readonly string[] Allowed =
    [
        Evento,
        SesionPrivada,
        Reunion,
        Bloqueo,
        Presupuesto,
        Otro
    ];

    private static readonly HashSet<string> BlockingTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        Evento,
        SesionPrivada,
        Bloqueo
    };

    public static bool TryNormalize(string? value, out string tipo)
    {
        tipo = Otro;
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

    public static bool BloqueaDisponibilidad(string? tipo)
    {
        return !string.IsNullOrWhiteSpace(tipo) && BlockingTypes.Contains(tipo.Trim());
    }
}
