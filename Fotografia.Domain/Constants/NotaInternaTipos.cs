namespace Fotografia.Domain.Constants;

public static class NotaInternaTipos
{
    public const string Cliente = "Cliente";
    public const string Pedido = "Pedido";
    public const string Evento = "Evento";
    public const string SesionPrivada = "SesionPrivada";
    public const string SolicitudPresupuesto = "SolicitudPresupuesto";
    public const string AgendaItem = "AgendaItem";

    private static readonly string[] Allowed =
    [
        Cliente,
        Pedido,
        Evento,
        SesionPrivada,
        SolicitudPresupuesto,
        AgendaItem
    ];

    public static bool TryNormalize(string? value, out string tipo)
    {
        tipo = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
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
