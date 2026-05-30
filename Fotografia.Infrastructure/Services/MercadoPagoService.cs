using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AutoMapper;
using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Pagos;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fotografia.Infrastructure.Services;

public sealed class MercadoPagoService(
    AppDbContext dbContext,
    HttpClient httpClient,
    IOptions<MercadoPagoSettings> options,
    IMapper mapper,
    ILogger<MercadoPagoService> logger) : IMercadoPagoService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly MercadoPagoSettings _settings = options.Value;

    public async Task<ApiResponse<MercadoPagoPreferenceResponseDto>> CreateCheckoutPreferenceAsync(
        Guid pedidoId,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
        {
            return ApiResponse<MercadoPagoPreferenceResponseDto>.Fail("Mercado Pago no esta configurado.");
        }

        var pedido = await dbContext.Pedidos
            .Include(x => x.Cliente)
            .Include(x => x.PedidoFotos)
            .ThenInclude(x => x.Foto)
            .FirstOrDefaultAsync(x => x.Id == pedidoId, cancellationToken);

        if (pedido is null)
        {
            return ApiResponse<MercadoPagoPreferenceResponseDto>.Fail("Pedido no encontrado.");
        }

        if (pedido.PedidoFotos.Count == 0)
        {
            return ApiResponse<MercadoPagoPreferenceResponseDto>.Fail("El pedido no tiene fotos.");
        }

        var payload = new
        {
            items = pedido.PedidoFotos.Select(item => new
            {
                title = item.Foto?.NombreArchivo ?? "Foto digital",
                quantity = item.Cantidad,
                unit_price = item.PrecioUnitario,
                currency_id = pedido.Moneda
            }),
            payer = new
            {
                name = pedido.Cliente?.Nombre,
                email = pedido.Cliente?.Email
            },
            back_urls = new
            {
                success = _settings.SuccessUrl,
                failure = _settings.FailureUrl,
                pending = _settings.PendingUrl
            },
            notification_url = _settings.NotificationUrl,
            external_reference = pedido.Id.ToString(),
            auto_return = "approved"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadopago.com/checkout/preferences");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
        request.Content = JsonContent.Create(payload, options: JsonOptions);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Mercado Pago rejected preference request with status {StatusCode}: {Body}", response.StatusCode, body);
            return ApiResponse<MercadoPagoPreferenceResponseDto>.Fail("No se pudo crear la preferencia de pago.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        var preferenceId = GetString(root, "id");
        var checkoutUrl = GetString(root, "init_point");
        var sandboxCheckoutUrl = GetString(root, "sandbox_init_point");

        if (string.IsNullOrWhiteSpace(preferenceId) || string.IsNullOrWhiteSpace(checkoutUrl))
        {
            return ApiResponse<MercadoPagoPreferenceResponseDto>.Fail("Mercado Pago no devolvio una preferencia valida.");
        }

        pedido.MercadoPagoPreferenceId = preferenceId;
        pedido.ActualizadoEnUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<MercadoPagoPreferenceResponseDto>.Ok(new MercadoPagoPreferenceResponseDto
        {
            PreferenceId = preferenceId,
            CheckoutUrl = checkoutUrl,
            SandboxCheckoutUrl = sandboxCheckoutUrl
        });
    }

    public async Task<ApiResponse<PagoResponseDto>> ProcessWebhookAsync(
        MercadoPagoWebhookDto request,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
        {
            return ApiResponse<PagoResponseDto>.Fail("Mercado Pago no esta configurado.");
        }

        var paymentId = request.Data?.Id;
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            return ApiResponse<PagoResponseDto>.Fail("Webhook sin payment id.");
        }

        using var paymentRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.mercadopago.com/v1/payments/{paymentId}");
        paymentRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);

        using var paymentResponse = await httpClient.SendAsync(paymentRequest, cancellationToken);
        if (!paymentResponse.IsSuccessStatusCode)
        {
            var body = await paymentResponse.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Mercado Pago rejected payment lookup with status {StatusCode}: {Body}", paymentResponse.StatusCode, body);
            return ApiResponse<PagoResponseDto>.Fail("No se pudo consultar el pago en Mercado Pago.");
        }

        await using var stream = await paymentResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        var externalReference = GetString(root, "external_reference");
        if (!Guid.TryParse(externalReference, out var pedidoId))
        {
            return ApiResponse<PagoResponseDto>.Fail("El pago no tiene una referencia de pedido valida.");
        }

        var pedido = await dbContext.Pedidos
            .Include(x => x.Pago)
            .FirstOrDefaultAsync(x => x.Id == pedidoId, cancellationToken);

        if (pedido is null)
        {
            return ApiResponse<PagoResponseDto>.Fail("Pedido no encontrado para el pago recibido.");
        }

        var status = GetString(root, "status") ?? "unknown";
        var preferenceId = GetString(root, "preference_id") ?? pedido.MercadoPagoPreferenceId;
        var amount = GetDecimal(root, "transaction_amount") ?? pedido.Total;
        var currency = GetString(root, "currency_id") ?? pedido.Moneda;
        var paidAt = GetDateTime(root, "date_approved");

        var pago = pedido.Pago ?? new Pago
        {
            PedidoId = pedido.Id,
            CreadoEnUtc = DateTime.UtcNow
        };

        pago.MercadoPagoPaymentId = paymentId;
        pago.MercadoPagoPreferenceId = preferenceId;
        pago.Estado = status;
        pago.Monto = amount;
        pago.Moneda = currency;
        pago.PagadoEnUtc = paidAt;
        pago.ActualizadoEnUtc = DateTime.UtcNow;

        if (pedido.Pago is null)
        {
            dbContext.Pagos.Add(pago);
        }

        pedido.Estado = string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase)
            ? "Pagado"
            : "Pago pendiente";
        pedido.ActualizadoEnUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PagoResponseDto>.Ok(mapper.Map<PagoResponseDto>(pago), "Webhook procesado.");
    }

    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_settings.AccessToken)
            && !_settings.AccessToken.StartsWith("__", StringComparison.Ordinal);
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
            ? property.GetString()
            : null;
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.TryGetDecimal(out var value)
            ? value
            : null;
    }

    private static DateTime? GetDateTime(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            && DateTime.TryParse(property.GetString(), out var value)
                ? value.ToUniversalTime()
                : null;
    }
}
