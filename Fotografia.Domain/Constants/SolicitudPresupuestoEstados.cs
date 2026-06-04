namespace Fotografia.Domain.Constants;

public static class SolicitudPresupuestoEstados
{
    public const string Nuevo = "Nuevo";
    public const string Contactado = "Contactado";
    public const string PresupuestoEnviado = "PresupuestoEnviado";
    public const string Aceptado = "Aceptado";
    public const string Rechazado = "Rechazado";
    public const string Cerrado = "Cerrado";

    private static readonly string[] Allowed =
    [
        Nuevo,
        Contactado,
        PresupuestoEnviado,
        Aceptado,
        Rechazado,
        Cerrado
    ];

    public static bool TryNormalize(string? value, out string estado)
    {
        estado = Nuevo;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var match = Allowed.FirstOrDefault(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            return false;
        }

        estado = match;
        return true;
    }
}
