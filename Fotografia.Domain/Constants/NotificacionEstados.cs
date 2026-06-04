namespace Fotografia.Domain.Constants;

public static class NotificacionEstados
{
    public const string Pendiente = "Pendiente";
    public const string Procesando = "Procesando";
    public const string Enviada = "Enviada";
    public const string Error = "Error";
    public const string Cancelada = "Cancelada";

    private static readonly string[] Allowed =
    [
        Pendiente,
        Procesando,
        Enviada,
        Error,
        Cancelada
    ];

    public static bool TryNormalize(string? value, out string estado)
    {
        estado = Pendiente;
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
