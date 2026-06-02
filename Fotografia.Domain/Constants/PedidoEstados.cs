namespace Fotografia.Domain.Constants;

public static class PedidoEstados
{
    public const string Pendiente = "Pendiente";
    public const string PendientePago = "PendientePago";
    public const string Pagado = "Pagado";
    public const string PreparandoDescarga = "PreparandoDescarga";
    public const string ListoParaDescargar = "ListoParaDescargar";
    public const string PagoPendiente = "Pago pendiente";
    public const string Aprobado = "Aprobado";
    public const string PagoAprobado = "PagoAprobado";
    public const string Cancelado = "Cancelado";
    public const string Reembolsado = "Reembolsado";

    private static readonly string[] Allowed =
    [
        Pendiente,
        PendientePago,
        Pagado,
        PreparandoDescarga,
        ListoParaDescargar,
        Cancelado,
        Reembolsado
    ];

    private static readonly HashSet<string> PaidStates = new(StringComparer.OrdinalIgnoreCase)
    {
        Pagado,
        PreparandoDescarga,
        ListoParaDescargar,
        Aprobado,
        PagoAprobado,
        "Pago aprobado",
        "approved"
    };

    private static readonly HashSet<string> DownloadStates = new(StringComparer.OrdinalIgnoreCase)
    {
        Pagado,
        PreparandoDescarga,
        ListoParaDescargar,
        Aprobado,
        PagoAprobado,
        "Pago aprobado",
        "approved"
    };

    private static readonly HashSet<string> FinalStates = new(StringComparer.OrdinalIgnoreCase)
    {
        Cancelado,
        Reembolsado
    };

    public static bool TryNormalize(string? value, out string estado)
    {
        estado = Pendiente;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();
        if (string.Equals(trimmed, PagoPendiente, StringComparison.OrdinalIgnoreCase))
        {
            estado = PendientePago;
            return true;
        }

        var match = Allowed.FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
        {
            estado = match;
            return true;
        }

        if (EsEstadoPagado(trimmed))
        {
            estado = Pagado;
            return true;
        }

        return false;
    }

    public static bool EsPagado(string? pedidoEstado, string? pagoEstado = null)
    {
        return EsEstadoPagado(pedidoEstado) || EsEstadoPagado(pagoEstado);
    }

    public static bool EsEstadoPagado(string? estado)
    {
        return !string.IsNullOrWhiteSpace(estado) && PaidStates.Contains(estado.Trim());
    }

    public static bool PermiteDescarga(string? pedidoEstado, string? pagoEstado = null)
    {
        return EsEstadoConDescarga(pedidoEstado) || EsEstadoConDescarga(pagoEstado);
    }

    public static bool EsFinal(string? estado)
    {
        return !string.IsNullOrWhiteSpace(estado) && FinalStates.Contains(estado.Trim());
    }

    public static bool EsPendientePago(string? estado)
    {
        return string.Equals(estado, Pendiente, StringComparison.OrdinalIgnoreCase)
            || string.Equals(estado, PendientePago, StringComparison.OrdinalIgnoreCase)
            || string.Equals(estado, PagoPendiente, StringComparison.OrdinalIgnoreCase);
    }

    private static bool EsEstadoConDescarga(string? estado)
    {
        return !string.IsNullOrWhiteSpace(estado) && DownloadStates.Contains(estado.Trim());
    }
}
