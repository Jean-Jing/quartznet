using Quartz.Impl.Triggers;
using Quartz.Spi;

namespace Quartz;

/// <summary>
/// CustomCalendarScheduleBuilder is a <see cref="IScheduleBuilder" />
/// that defines calendar time (day, week, month, year) interval-based
/// schedules for Triggers.
/// </summary>
/// <remarks>
/// <para>
/// Quartz provides a builder-style API for constructing scheduling-related
/// entities via a Domain-Specific Language (DSL).  The DSL can best be
/// utilized through the usage of static imports of the methods on the classes
/// <see cref="TriggerBuilder" />, <see cref="JobBuilder" />,
/// <see cref="DateBuilder" />, <see cref="JobKey" />, <see cref="TriggerKey" />
/// and the various <see cref="IScheduleBuilder" /> implementations.
/// </para>
/// <para>Client code can then use the DSL to write code such as this:</para>
/// <code>
/// JobDetail job = JobBuilder.Create&lt;MyJob&gt;()
///     .WithIdentity("myJob")
///     .Build();
/// Trigger trigger = TriggerBuilder.Create()
///     .WithIdentity("myTrigger", "myTriggerGroup")
///     .WithSimpleSchedule(x => x
///         .WithIntervalInHours(1)
///         .RepeatForever())
///     .StartAt(DateBuilder.FutureDate(10, IntervalUnit.Minute))
///     .Build();
/// scheduler.scheduleJob(job, trigger);
/// </code>
/// </remarks>
/// <seealso cref="ICustomCalendarTrigger" />
/// <seealso cref="CronScheduleBuilder" />
/// <seealso cref="IScheduleBuilder" />
/// <seealso cref="SimpleScheduleBuilder" />
/// <seealso cref="TriggerBuilder" />
public sealed class CustomCalendarScheduleBuilder : ScheduleBuilder<ICustomCalendarTrigger>
{
    private int repeatCount = 1;
    private int interval = 1;
    private IntervalUnit intervalUnit = IntervalUnit.Day;

    private int misfireInstruction = MisfireInstruction.SmartPolicy;
    private TimeZoneInfo? timeZone;

    private int byMonth;
    private string? byMonthDay; // 1,2,3,...,30,31
    private string? byDay; // MO, 1MO, 2SU, 5FR, -1SA

    /// <summary>
    /// Set the interval at which the <see cref="ICustomCalendarTrigger" /> will repeat.
    /// </summary>

    private CustomCalendarScheduleBuilder()
    {
    }

    /// <summary>
    /// Create a CustomCalendarScheduleBuilder.
    /// </summary>
    /// <returns></returns>
    public static CustomCalendarScheduleBuilder Create()
    {
        return new CustomCalendarScheduleBuilder();
    }

    /// <summary>
    /// Build the actual Trigger -- NOT intended to be invoked by end users,
    /// but will rather be invoked by a TriggerBuilder which this
    /// ScheduleBuilder is given to.
    /// </summary>
    /// <returns></returns>
    public override IMutableTrigger Build()
    {
        CustomCalendarTriggerImpl st = new CustomCalendarTriggerImpl();

        st.RepeatCount = repeatCount;
        st.RepeatInterval = interval;
        st.RepeatIntervalUnit = intervalUnit;
        st.MisfireInstruction = misfireInstruction;
        st.timeZone = timeZone;

        st.ByMonth = byMonth;
        st.ByMonthDay = byMonthDay;
        st.ByDay = byDay;

        return st;
    }


    public CustomCalendarScheduleBuilder WithRepeatCount(int repeatCount)
    {
        this.repeatCount = repeatCount;
        return this;
    }

    /// <summary>
    /// Specify the time unit and interval for the Trigger to be produced.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="interval">the interval at which the trigger should repeat.</param>
    /// <param name="unit"> the time unit (IntervalUnit) of the interval.</param>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="ICustomCalendarTrigger.RepeatInterval" />
    /// <seealso cref="ICustomCalendarTrigger.RepeatIntervalUnit" />
    public CustomCalendarScheduleBuilder WithInterval(int interval, IntervalUnit unit)
    {
        ValidateInterval(interval);
        this.interval = interval;
        intervalUnit = unit;
        return this;
    }

    /// <summary>
    /// Specify an interval in the IntervalUnit.SECOND that the produced
    /// Trigger will repeat at.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="intervalInSeconds">the number of seconds at which the trigger should repeat.</param>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="ICustomCalendarTrigger.RepeatInterval" />
    /// <seealso cref="ICustomCalendarTrigger.RepeatIntervalUnit" />
    public CustomCalendarScheduleBuilder WithIntervalInSeconds(int intervalInSeconds)
    {
        ValidateInterval(intervalInSeconds);
        interval = intervalInSeconds;
        intervalUnit = IntervalUnit.Second;
        return this;
    }

    /// <summary>
    /// Specify an interval in the IntervalUnit.MINUTE that the produced
    /// Trigger will repeat at.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="intervalInMinutes">the number of minutes at which the trigger should repeat.</param>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="ICustomCalendarTrigger.RepeatInterval" />
    /// <seealso cref="ICustomCalendarTrigger.RepeatIntervalUnit" />
    public CustomCalendarScheduleBuilder WithIntervalInMinutes(int intervalInMinutes)
    {
        ValidateInterval(intervalInMinutes);
        interval = intervalInMinutes;
        intervalUnit = IntervalUnit.Minute;
        return this;
    }

    /// <summary>
    /// Specify an interval in the IntervalUnit.HOUR that the produced
    /// Trigger will repeat at.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="intervalInHours">the number of hours at which the trigger should repeat.</param>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="ICustomCalendarTrigger.RepeatInterval" />
    /// <seealso cref="ICustomCalendarTrigger.RepeatIntervalUnit" />
    public CustomCalendarScheduleBuilder WithIntervalInHours(int intervalInHours)
    {
        ValidateInterval(intervalInHours);
        interval = intervalInHours;
        intervalUnit = IntervalUnit.Hour;
        return this;
    }

    /// <summary>
    /// Specify an interval in the IntervalUnit.DAY that the produced``
    /// Trigger will repeat at.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="intervalInDays">the number of days at which the trigger should repeat.</param>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="ICustomCalendarTrigger.RepeatInterval" />
    /// <seealso cref="ICustomCalendarTrigger.RepeatIntervalUnit" />
    public CustomCalendarScheduleBuilder WithIntervalInDays(int intervalInDays)
    {
        ValidateInterval(intervalInDays);
        interval = intervalInDays;
        intervalUnit = IntervalUnit.Day;
        return this;
    }

    /// <summary>
    /// Specify an interval in the IntervalUnit.WEEK that the produced
    /// Trigger will repeat at.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="intervalInWeeks">the number of weeks at which the trigger should repeat.</param>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="ICustomCalendarTrigger.RepeatInterval" />
    /// <seealso cref="ICustomCalendarTrigger.RepeatIntervalUnit" />
    public CustomCalendarScheduleBuilder WithIntervalInWeeks(int intervalInWeeks)
    {
        ValidateInterval(intervalInWeeks);
        interval = intervalInWeeks;
        intervalUnit = IntervalUnit.Week;
        return this;
    }

    /// <summary>
    /// Specify an interval in the IntervalUnit.MONTH that the produced
    /// Trigger will repeat at.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="intervalInMonths">the number of months at which the trigger should repeat.</param>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="ICustomCalendarTrigger.RepeatInterval" />
    /// <seealso cref="ICustomCalendarTrigger.RepeatIntervalUnit" />
    public CustomCalendarScheduleBuilder WithIntervalInMonths(int intervalInMonths)
    {
        ValidateInterval(intervalInMonths);
        interval = intervalInMonths;
        intervalUnit = IntervalUnit.Month;
        return this;
    }

    /// <summary>
    /// Specify an interval in the IntervalUnit.YEAR that the produced
    /// Trigger will repeat at.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="intervalInYears">the number of years at which the trigger should repeat.</param>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="ICustomCalendarTrigger.RepeatInterval" />
    /// <seealso cref="ICustomCalendarTrigger.RepeatIntervalUnit" />
    public CustomCalendarScheduleBuilder WithIntervalInYears(int intervalInYears)
    {
        ValidateInterval(intervalInYears);
        interval = intervalInYears;
        intervalUnit = IntervalUnit.Year;
        return this;
    }

    /// <summary>
    /// If the Trigger misfires, use the
    /// <see cref="MisfireInstruction.IgnoreMisfirePolicy" /> instruction.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="MisfireInstruction.IgnoreMisfirePolicy" />
    public CustomCalendarScheduleBuilder WithMisfireHandlingInstructionIgnoreMisfires()
    {
        misfireInstruction = MisfireInstruction.IgnoreMisfirePolicy;
        return this;
    }


    /// <summary>
    /// If the Trigger misfires, use the
    /// <see cref="MisfireInstruction.CalendarIntervalTrigger.DoNothing" /> instruction.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="MisfireInstruction.CalendarIntervalTrigger.DoNothing" />
    public CustomCalendarScheduleBuilder WithMisfireHandlingInstructionDoNothing()
    {
        misfireInstruction = MisfireInstruction.CalendarIntervalTrigger.DoNothing;
        return this;
    }

    /// <summary>
    /// If the Trigger misfires, use the
    /// <see cref="MisfireInstruction.CalendarIntervalTrigger.FireOnceNow" /> instruction.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="MisfireInstruction.CalendarIntervalTrigger.FireOnceNow" />
    public CustomCalendarScheduleBuilder WithMisfireHandlingInstructionFireAndProceed()
    {
        misfireInstruction = MisfireInstruction.CalendarIntervalTrigger.FireOnceNow;
        return this;
    }

    /// <summary>
    /// TimeZone in which to base the schedule.
    /// </summary>
    /// <param name="timezone">the time-zone for the schedule</param>
    /// <returns>the updated CustomCalendarScheduleBuilder</returns>
    /// <seealso cref="ICustomCalendarTrigger.TimeZone" />
    public CustomCalendarScheduleBuilder InTimeZone(TimeZoneInfo? timezone)
    {
        timeZone = timezone;
        return this;
    }

    // ReSharper disable once UnusedParameter.Local
    private static void ValidateInterval(int interval)
    {
        if (interval <= 0)
        {
            ThrowHelper.ThrowArgumentException("Interval must be a positive value.");
        }
    }

    internal CustomCalendarScheduleBuilder WithMisfireHandlingInstruction(int readMisfireInstructionFromString)
    {
        misfireInstruction = readMisfireInstructionFromString;
        return this;
    }

    private static void ValidateMonth(int month, IntervalUnit intervalUnit)
    {
        if (intervalUnit != IntervalUnit.Year && intervalUnit != IntervalUnit.Month)
        {
            ThrowHelper.ThrowArgumentException("ByMonth must be used with Year/Month Interval Unit.");
        }
        if (month < 1 || month > 12)
        {
            ThrowHelper.ThrowArgumentException("Month must be a valid value.");
        }
    }

    private static void ValidateMonthDay(string monthDay, IntervalUnit intervalUnit)
    {
        if (intervalUnit != IntervalUnit.Year && intervalUnit != IntervalUnit.Month)
        {
            ThrowHelper.ThrowArgumentException("ByMonthDay must be used with Year/Month Interval Unit.");
        }
    }

    private static void ValidateDay(string day, IntervalUnit intervalUnit)
    {
        if (intervalUnit != IntervalUnit.Year && intervalUnit != IntervalUnit.Month && intervalUnit != IntervalUnit.Week)
        {
            ThrowHelper.ThrowArgumentException("ByDay must be used with Year/Month/Week Interval Unit.");
        }
    }
    public CustomCalendarScheduleBuilder ByMonth(int month)
    {
        ValidateMonth(month, intervalUnit);
        byMonth = month;
        return this;
    }

    public CustomCalendarScheduleBuilder ByMonthDay(string monthDay)
    {
        ValidateMonthDay(monthDay, intervalUnit);
        byMonthDay = monthDay;
        return this;
    }

    public CustomCalendarScheduleBuilder ByDay(string day)
    {
        ValidateDay(day, intervalUnit);
        byDay = day;
        return this;
    }
}

/// <summary>
/// Extension methods that attach <see cref="CustomCalendarScheduleBuilder" /> to <see cref="TriggerBuilder" />.
/// </summary>
public static class CustomCalendarTriggerBuilderExtensions
{
    public static TriggerBuilder WithCustomCalendarSchedule(this TriggerBuilder triggerBuilder)
    {
        CustomCalendarScheduleBuilder builder = CustomCalendarScheduleBuilder.Create();
        return triggerBuilder.WithSchedule(builder);
    }

    public static TriggerBuilder WithCustomCalendarSchedule(this TriggerBuilder triggerBuilder, Action<CustomCalendarScheduleBuilder> action)
    {
        CustomCalendarScheduleBuilder builder = CustomCalendarScheduleBuilder.Create();
        action(builder);
        return triggerBuilder.WithSchedule(builder);
    }
}