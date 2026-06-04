namespace Fotografia.Domain.Constants;

public static class BitacoraSeveridades
{
    public const string Info = "Info";
    public const string Warning = "Warning";
    public const string Error = "Error";
    public const string Critical = "Critical";

    public static bool IsValid(string? value)
    {
        return string.Equals(value, Info, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, Warning, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, Error, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, Critical, StringComparison.OrdinalIgnoreCase);
    }

    public static string Normalize(string? value)
    {
        if (string.Equals(value, Warning, StringComparison.OrdinalIgnoreCase))
        {
            return Warning;
        }

        if (string.Equals(value, Error, StringComparison.OrdinalIgnoreCase))
        {
            return Error;
        }

        if (string.Equals(value, Critical, StringComparison.OrdinalIgnoreCase))
        {
            return Critical;
        }

        return Info;
    }
}
