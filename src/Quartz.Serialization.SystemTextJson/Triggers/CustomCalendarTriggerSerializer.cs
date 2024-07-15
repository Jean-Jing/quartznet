using System.Text.Json;

using Quartz.Util;

namespace Quartz.Triggers;

internal sealed class CustomCalendarTriggerSerializer : TriggerSerializer<ICustomCalendarTrigger>
{
    public static CustomCalendarTriggerSerializer Instance { get; } = new();

    private CustomCalendarTriggerSerializer()
    {
    }

    public const string TriggerTypeKey = "CustomCalendarTrigger";

    public override string TriggerTypeForJson => TriggerTypeKey;

    public override IScheduleBuilder CreateScheduleBuilder(JsonElement jsonElement)
    {
        var repeatCount = jsonElement.GetProperty("RepeatCount").GetInt32();
        var repeatIntervalUnit = jsonElement.GetProperty("RepeatIntervalUnit").GetEnum<IntervalUnit>();
        var repeatInterval = jsonElement.GetProperty("RepeatInterval").GetInt32();
        var timeZone = jsonElement.GetProperty("TimeZone").GetTimeZone();

        var byMonth = jsonElement.GetProperty("ByMonth").GetInt32();
        var byMonthDay = jsonElement.GetProperty("ByMonthDay").GetString();
        var byDay = jsonElement.GetProperty("ByDay").GetString();

        var sb = CustomCalendarScheduleBuilder.Create()
            .WithRepeatCount(repeatCount)
            .WithInterval(repeatInterval, repeatIntervalUnit)
            .InTimeZone(timeZone);
        if (byMonth > 0 && byMonth <= 12)
        {
            sb = sb.ByMonth(byMonth);
        }

        if (!string.IsNullOrEmpty(byMonthDay))
        {
            sb = sb.ByMonthDay(byMonthDay);
        }

        if (!string.IsNullOrEmpty(byDay))
        {
            sb = sb.ByDay(byDay);
        }

        return sb;
    }

    protected override void SerializeFields(Utf8JsonWriter writer, ICustomCalendarTrigger trigger)
    {
        writer.WriteNumber("RepeatCount", trigger.RepeatCount);
        writer.WriteNumber("RepeatInterval", trigger.RepeatInterval);
        writer.WriteEnum("RepeatIntervalUnit", trigger.RepeatIntervalUnit);
        writer.WriteNumber("ByMonth", trigger.ByMonth);
        writer.WriteString("ByMonthDay", trigger.ByMonthDay);
        writer.WriteString("ByDay", trigger.ByDay);
        writer.WriteTimeZoneInfo("TimeZone", trigger.TimeZone);
    }
}