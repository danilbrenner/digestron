namespace Digestron.Infra.Options;

public sealed class ScheduleOptions
{
    public const string SectionName = "Schedule";

    public string[] DeliveryTimesUtc { get; init; } = [];
}
