namespace Fotografia.Domain.Constants;

public static class PedidoEstados
{
    public const string Pendiente = "Pendiente";
    public const string PendientePago = "PendientePago";
    public const string Pagado = "Pagado";
    public const string PagoPendiente = "Pago pendiente";
    public const string Aprobado = "Aprobado";
    public const string PagoAprobado = "PagoAprobado";
    public const string Cancelado = "Cancelado";
    public const string Reembolsado = "Reembolsado";

    private static readonly HashSet<string> PaidStates = new(StringComparer.OrdinalIgnoreCase)
    {
        Pagado,
        Aprobado,
        PagoAprobado,
        "Pago aprobado",
        "approved"
    };

    public static bool EsPagado(string? pedidoEstado, string? pagoEstado = null)
    {
        return EsEstadoPagado(pedidoEstado) || EsEstadoPagado(pagoEstado);
    }

    public static bool EsEstadoPagado(string? estado)
    {
        return !string.IsNullOrWhiteSpace(estado) && PaidStates.Contains(estado.Trim());
    }
}
