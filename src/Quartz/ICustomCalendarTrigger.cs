namespace Quartz;

/// <summary>
///  A <see cref="ITrigger" /> that is used to fire a <see cref="IJobDetail" />
///  based upon repeating calendar time intervals.
///  </summary>
public interface ICustomCalendarTrigger : ITrigger
{
    /// <summary>
    /// Get the number of times for interval this trigger should repeat,
    /// after which it will be automatically deleted.
    /// </summary>
    int RepeatCount { get; } // Long1

    /// <summary>
    /// Get or set the interval unit - the time unit on with the interval applies.
    /// </summary>
    IntervalUnit RepeatIntervalUnit { get; set; } // String1

    /// <summary>
    /// Get the time interval that will be added to the <see cref="ICustomCalendarTrigger" />'s
    /// fire time (in the set repeat interval unit) in order to calculate the time of the
    /// next trigger repeat.
    /// </summary>
    int RepeatInterval { get; set; } // Int1

    /// <summary>
    /// Get the time interval that will be added to the <see cref="ICustomCalendarTrigger" />'s
    /// fire time (in the set repeat interval unit) in order to calculate the time of the
    /// next trigger repeat.
    /// </summary>
    int ByMonth { get; set; } // Long2

    /// <summary>
    /// Get the time interval that will be added to the <see cref="ICustomCalendarTrigger" />'s
    /// fire time (in the set repeat interval unit) in order to calculate the time of the
    /// next trigger repeat.
    /// </summary>
    string? ByMonthDay { get; set; } // String2

    /// <summary>
    /// Get the time interval that will be added to the <see cref="ICustomCalendarTrigger" />'s
    /// fire time (in the set repeat interval unit) in order to calculate the time of the
    /// next trigger repeat.
    /// </summary>
    string? ByDay { get; set; } // String3

    /// <summary>
    /// Get the number of times the <see cref="ICustomCalendarTrigger" /> has already fired.
    /// </summary>
    int TimesTriggered { get; set; } // Int2

    /// <summary>
    /// Gets the time zone within which time calculations related to this trigger will be performed.
    /// </summary>
    /// <remarks>
    /// If null, the system default TimeZone will be used.
    /// </remarks>
    TimeZoneInfo TimeZone { get; }
}