namespace Fotografia.Application.Helpers;

public static class WhatsAppHelper
{
    private const string DefaultMessage = "Hola, quiero consultar por una sesion de fotos";

    public static string? CreateWhatsAppUrl(string? whatsApp, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(whatsApp))
        {
            return null;
        }

        var number = new string(whatsApp.Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(number))
        {
            return null;
        }

        var encodedMessage = Uri.EscapeDataString(
            string.IsNullOrWhiteSpace(message) ? DefaultMessage : message.Trim());

        return $"https://wa.me/{number}?text={encodedMessage}";
    }
}
