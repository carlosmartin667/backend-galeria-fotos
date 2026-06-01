namespace Fotografia.Domain.Constants;

public static class AgendaItemEstados
{
    public const string Programado = "Programado";
    public const string Confirmado = "Confirmado";
    public const string Cancelado = "Cancelado";
    public const string Finalizado = "Finalizado";

    private static readonly string[] Allowed =
    [
        Programado,
        Confirmado,
        Cancelado,
        Finalizado
    ];

    public static bool TryNormalize(string? value, out string estado)
    {
        estado = Programado;
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

    public static bool EsCancelado(string? estado)
    {
        return string.Equals(estado, Cancelado, StringComparison.OrdinalIgnoreCase);
    }
}
