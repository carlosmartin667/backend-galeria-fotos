namespace Fotografia.Infrastructure.Settings;

public sealed class NotificationsSettings
{
    public const string SectionName = "Notifications";

    public bool Enabled { get; set; }
    public bool WorkerEnabled { get; set; }
    public string DefaultFromEmail { get; set; } = "no-reply@example.com";
    public NotificationsWorkerSettings Worker { get; set; } = new();
}

public sealed class NotificationsWorkerSettings
{
    public int IntervalSeconds { get; set; } = 30;
    public int BatchSize { get; set; } = 20;
    public int MaxAttempts { get; set; } = 3;
}
