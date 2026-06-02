namespace Fotografia.Domain.Constants;

public static class SesionPrivadaEstados
{
    public const string Borrador = "Borrador";
    public const string Programada = "Programada";
    public const string EnEdicion = "EnEdicion";
    public const string ListaParaCliente = "ListaParaCliente";
    public const string Publicada = "Publicada";
    public const string Finalizada = "Finalizada";
    public const string Cancelada = "Cancelada";

    public const string LegacyActiva = "Activa";

    private static readonly string[] Allowed =
    [
        Borrador,
        Programada,
        EnEdicion,
        ListaParaCliente,
        Publicada,
        Finalizada,
        Cancelada
    ];

    public static bool TryNormalize(string? value, out string estado)
    {
        estado = Programada;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();
        if (string.Equals(trimmed, LegacyActiva, StringComparison.OrdinalIgnoreCase))
        {
            estado = Programada;
            return true;
        }

        var match = Allowed.FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            return false;
        }

        estado = match;
        return true;
    }
}
