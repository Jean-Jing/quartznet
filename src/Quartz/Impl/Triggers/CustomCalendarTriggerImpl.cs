#region License

/*
 * All content copyright Marko Lahma, unless otherwise indicated. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy
 * of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 */

#endregion

using Ical.Net.DataTypes;
using Ical.Net.Evaluation;

namespace Quartz.Impl.Triggers;

/// <summary>
///  A concrete <see cref="ITrigger" /> that is used to fire a <see cref="IJobDetail" />
///  based upon repeating calendar time intervals.
///  </summary>
/// <remarks>
/// The trigger will fire every N (see <see cref="RepeatInterval" />) units of calendar time
/// (see <see cref="RepeatIntervalUnit" />) as specified in the trigger's definition.
/// This trigger can achieve schedules that are not possible with <see cref="ISimpleTrigger" /> (e.g
/// because months are not a fixed number of seconds) or <see cref="ICronTrigger" /> (e.g. because
/// "every 5 months" is not an even divisor of 12).
/// <para>
/// If you use an interval unit of <see cref="IntervalUnit.Month" /> then care should be taken when setting
/// a <see cref="startTimeUtc" /> value that is on a day near the end of the month.  For example,
/// if you choose a start time that occurs on January 31st, and have a trigger with unit
/// <see cref="IntervalUnit.Month" /> and interval 1, then the next fire time will be February 28th,
/// and the next time after that will be March 28th - and essentially each subsequent firing will
/// occur on the 28th of the month, even if a 31st day exists.  If you want a trigger that always
/// fires on the last day of the month - regardless of the number of days in the month,
/// you should use <see cref="ICronTrigger" />.
/// </para>
/// </remarks>
/// <see cref="ITrigger" />
/// <see cref="ICronTrigger" />
/// <see cref="ISimpleTrigger" />
/// <see cref="IDailyTimeIntervalTrigger" />
/// <see cref="ICalendarIntervalTrigger" />
/// <since>2.0</since>
/// <author>James House</author>
/// <author>Marko Lahma (.NET)</author>
[Serializable]
public sealed class CustomCalendarTriggerImpl : AbstractTrigger, ICustomCalendarTrigger
{
    public const int RepeatIndefinitely = -1;

    private DateTimeOffset startTimeUtc;
    private DateTimeOffset? endTimeUtc;
    private DateTimeOffset? nextFireTimeUtc; // Making a public property which called GetNextFireTime/SetNextFireTime would make the json attribute unnecessary
    private DateTimeOffset? previousFireTimeUtc; // Making a public property which called GetPreviousFireTime/SetPreviousFireTime would make the json attribute unnecessary
    
    private int repeatInterval = 1;

    private int repeatCount = RepeatIndefinitely;
    private int byMonth = 0; // 1 ~ 12
    private string? byMonthDay = ""; // 1,2,3,...,30,31
    private string? byDay = string.Empty; // 1MO, -1TU(Last), 2WE (Monthly / Yearly) / MO (Weekly)

    internal TimeZoneInfo? timeZone;

    // Serializing TimeZones is tricky in .NET Core. This helper will ensure that we get the same timezone on a given platform,
    // but there's not yet a good method of serializing/deserializing timezones cross-platform since Windows timezone IDs don't
    // match IANA tz IDs (https://en.wikipedia.org/wiki/List_of_tz_database_time_zones). This feature is coming, but depending
    // on timelines, it may be worth doing the mapping here.
    // More info: https://github.com/dotnet/corefx/issues/7757
    private string? timeZoneInfoId
    {
        get => timeZone?.Id;
        set => timeZone = value == null ? null : TimeZoneInfo.FindSystemTimeZoneById(value);
    }

    public CustomCalendarTriggerImpl() : base(TimeProvider.System)
    {
    }

    /// <summary>
    /// Create a <see cref="ICustomCalendarTrigger" /> with no settings.
    /// </summary>
    /// <param name="timeProvider">Time provider instance to use, defaults to <see cref="TimeProvider.System"/></param>
    public CustomCalendarTriggerImpl(TimeProvider timeProvider) : base(timeProvider)
    {
        StartTimeUtc = TimeProvider.GetUtcNow();
        TimeZone = TimeZoneInfo.Local;
    }

    /// <summary>
    /// Create a <see cref="CustomCalendarTriggerImpl" /> that will occur immediately, and
    /// repeat at the given interval.
    /// </summary>
    /// <param name="name">Name for the trigger instance.</param>
    /// <param name="intervalUnit">The repeat interval unit (minutes, days, months, etc).</param>
    /// <param name="repeatInterval">The number of milliseconds to pause between the repeat firing.</param>
    /// <param name="timeProvider">Time provider instance to use, defaults to <see cref="TimeProvider.System"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    public CustomCalendarTriggerImpl(string name, IntervalUnit intervalUnit, 
            int repeatInterval, 
            TimeProvider? timeProvider = null)
        : this(name, SchedulerConstants.DefaultGroup, intervalUnit, repeatInterval, timeProvider)
    {
        StartTimeUtc = TimeProvider.GetUtcNow();
        TimeZone = TimeZoneInfo.Local;
    }

    /// <summary>
    /// Create a <see cref="ICustomCalendarTrigger" /> that will occur immediately, and
    /// repeat at the given interval
    /// </summary>
    /// <param name="name">Name for the trigger instance.</param>
    /// <param name="group">Group for the trigger instance.</param>
    /// <param name="intervalUnit">The repeat interval unit (minutes, days, months, etc).</param>
    /// <param name="repeatInterval">The number of milliseconds to pause between the repeat firing.</param>
    /// <param name="timeProvider">Time provider instance to use, defaults to <see cref="TimeProvider.System"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="group"/> are <see langword="null"/>.</exception>
    public CustomCalendarTriggerImpl(
        string name,
        string group,
        IntervalUnit intervalUnit,
        int repeatInterval,
        TimeProvider? timeProvider = null)
        : this(name, group, (timeProvider ?? TimeProvider.System).GetUtcNow(), endTime: null, intervalUnit, repeatInterval, timeProvider)
    {
    }

    /// <summary>
    /// Create a <see cref="ICustomCalendarTrigger" /> that will occur at the given time,
    /// and repeat at the given interval until the given end time.
    /// </summary>
    /// <param name="name">Name for the trigger instance.</param>
    /// <param name="startTimeUtc">A <see cref="DateTimeOffset" /> set to the time for the <see cref="ITrigger" /> to fire.</param>
    /// <param name="endTime">A <see cref="DateTimeOffset" /> set to the time for the <see cref="ITrigger" /> to quit repeat firing.</param>
    /// <param name="intervalUnit">The repeat interval unit (minutes, days, months, etc).</param>
    /// <param name="repeatInterval">The number of milliseconds to pause between the repeat firing.</param>
    /// <param name="timeProvider">Time provider instance to use, defaults to <see cref="TimeProvider.System"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    public CustomCalendarTriggerImpl(
        string name,
        DateTimeOffset startTimeUtc,
        DateTimeOffset? endTime,
        IntervalUnit intervalUnit,
        int repeatInterval,
        TimeProvider? timeProvider = null)
        : this(name, SchedulerConstants.DefaultGroup, startTimeUtc, endTime, intervalUnit, repeatInterval, timeProvider)
    {
    }

    /// <summary>
    /// Create a <see cref="ICustomCalendarTrigger" /> that will occur at the given time,
    /// and repeat at the given interval until the given end time.
    /// </summary>
    /// <param name="name">Name for the trigger instance.</param>
    /// <param name="group">Group for the trigger instance.</param>
    /// <param name="startTimeUtc">A <see cref="DateTimeOffset" /> set to the time for the <see cref="ITrigger" /> to fire.</param>
    /// <param name="endTime">A <see cref="DateTimeOffset" /> set to the time for the <see cref="ITrigger" /> to quit repeat firing.</param>
    /// <param name="intervalUnit">The repeat interval unit (minutes, days, months, etc).</param>
    /// <param name="repeatInterval">The number of milliseconds to pause between the repeat firing.</param>
    /// <param name="timeProvider">Time provider instance to use, defaults to <see cref="TimeProvider.System"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="group"/> are <see langword="null"/>.</exception>
    public CustomCalendarTriggerImpl(string name,
        string group,
        DateTimeOffset startTimeUtc,
        DateTimeOffset? endTime,
        IntervalUnit intervalUnit,
        int repeatInterval,
        TimeProvider? timeProvider = null)
        : base(name, group, timeProvider ?? TimeProvider.System)
    {
        if (startTimeUtc == DateTimeOffset.MinValue)
        {
            startTimeUtc = TimeProvider.GetUtcNow();
        }
        StartTimeUtc = startTimeUtc;

        if (endTime.HasValue)
        {
            EndTimeUtc = endTime;
        }
        if (timeZone == null)
        {
            TimeZone = TimeZoneInfo.Local;
        }
        else
        {
            TimeZone = timeZone;
        }
        RepeatIntervalUnit = intervalUnit;
        RepeatInterval = repeatInterval;
    }

    /// <summary>
    /// Create a <see cref="ICustomCalendarTrigger" /> that will occur at the given time,
    /// and repeat at the given interval until the given end time.
    /// </summary>
    /// <param name="name">Name for the trigger instance.</param>
    /// <param name="group">Group for the trigger instance.</param>
    /// <param name="jobName">Name of the associated job.</param>
    /// <param name="jobGroup">Group of the associated job.</param>
    /// <param name="startTimeUtc">A <see cref="DateTimeOffset" /> set to the time for the <see cref="ITrigger" /> to fire.</param>
    /// <param name="endTime">A <see cref="DateTimeOffset" /> set to the time for the <see cref="ITrigger" /> to quit repeat firing.</param>
    /// <param name="intervalUnit">The repeat interval unit (minutes, days, months, etc).</param>
    /// <param name="repeatInterval">The number of milliseconds to pause between the repeat firing.</param>
    /// <param name="repeatCount">The number of counts to repeat.</param>
    /// <param name="byMonth">The month of the year to fire on.</param>
    /// <param name="byMonthDay">The month day of the month to fire on.</param>
    /// <param name="byDay">The day of the week to fire on.</param>
    /// <param name="timeProvider">Time provider instance to use, defaults to <see cref="TimeProvider.System"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/>, <paramref name="group"/>, <paramref name="jobName"/> or <paramref name="jobGroup"/> are <see langword="null"/>.</exception>
    public CustomCalendarTriggerImpl(
        string name,
        string group,
        string jobName,
        string jobGroup,
        DateTimeOffset startTimeUtc,
        DateTimeOffset? endTime,
        IntervalUnit intervalUnit,
        int repeatInterval,
        int repeatCount,
        int byMonth,
        string byMonthDay,
        string byDay,
        TimeProvider? timeProvider = null)
        : base(name, group, jobName, jobGroup, timeProvider ?? TimeProvider.System)
    {
        if (startTimeUtc == DateTimeOffset.MinValue)
        {
            startTimeUtc = TimeProvider.GetUtcNow();
        }
        StartTimeUtc = startTimeUtc;

        if (endTime.HasValue)
        {
            EndTimeUtc = endTime;
        }
        if (timeZone == null)
        {
            TimeZone = TimeZoneInfo.Local;
        }
        else
        {
            TimeZone = timeZone;
        }
        
        RepeatIntervalUnit = intervalUnit;
        RepeatInterval = repeatInterval;
        RepeatCount = repeatCount;
        
        ByMonth = byMonth;
        ByMonthDay = byMonthDay;
        ByDay = byDay;
    }

    /// <summary>
    /// Get the number of times for interval this trigger should repeat,
    /// after which it will be automatically deleted.
    /// </summary>
    public int RepeatCount
    {
        get => repeatCount;
        set
        {
            if (value < 0 && value != RepeatIndefinitely)
            {
                ThrowHelper.ThrowArgumentException("Repeat count must be >= 0, use the constant RepeatIndefinitely for infinite.");
            }

            repeatCount = value;
        }
    }

    /// <summary>
    /// Get the time at which the <see cref="ICustomCalendarTrigger" /> should occur.
    /// </summary>
    public override DateTimeOffset StartTimeUtc
    {
        get => startTimeUtc;
        set
        {
            DateTimeOffset? eTime = EndTimeUtc;
            if (eTime.HasValue && eTime.Value < value)
            {
                ThrowHelper.ThrowArgumentException("End time cannot be before start time");
            }

            // round off millisecond...
            DateTimeOffset dt = new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Offset);
            startTimeUtc = dt;
        }
    }

    /// <summary>
    /// Tells whether this Trigger instance can handle events
    /// in millisecond precision.
    /// </summary>
    public override bool HasMillisecondPrecision => true;

    /// <summary>
    /// Get the time at which the <see cref="ICustomCalendarTrigger" /> should quit
    /// repeating.
    /// </summary>
    public override DateTimeOffset? EndTimeUtc
    {
        get => endTimeUtc;
        set
        {
            DateTimeOffset sTime = StartTimeUtc;
            if (value.HasValue && sTime > value.Value)
            {
                ThrowHelper.ThrowArgumentException("End time cannot be before start time");
            }

            endTimeUtc = value;
        }
    }

    /// 

    /// <summary>
    /// Get or set the interval unit - the time unit on with the interval applies.
    /// </summary>
    public IntervalUnit RepeatIntervalUnit { get; set; } = IntervalUnit.Day;

    /// <summary>
    /// Get the time interval that will be added to the <see cref="ICustomCalendarTrigger" />'s
    /// fire time (in the set repeat interval unit) in order to calculate the time of the
    /// next trigger repeat.
    /// </summary>
    public int RepeatInterval
    {
        get => repeatInterval;
        set
        {
            if (value < 0)
            {
                ThrowHelper.ThrowArgumentException("Repeat interval must be >= 1");
            }

            repeatInterval = value;
        }
    }

    /// <summary>
    /// Gets or sets the month, 1-12.
    /// </summary>
    public int ByMonth
    {
        get => byMonth;
        set => byMonth = value;
    }

    /// <summary>
    /// Gets or sets the month day, valid values are 1-31.
    /// </summary>
    public string? ByMonthDay
    {
        get => byMonthDay;
        set => byMonthDay = value;
    }

    /// <summary>
    /// Gets or sets the day of the week, valid values are 1MO, 2TU, 3WE, 4TH, 5FR, 6SA, 7SU.
    /// </summary>
    public string? ByDay
    {
        get => byDay;
        set => byDay = value;
    }

    /// <summary>
    /// Gets or sets the time zone.
    /// </summary>
    public TimeZoneInfo TimeZone
    {
        get
        {
            if (timeZone == null)
            {
                timeZone = TimeZoneInfo.Local;
            }
            return timeZone;
        }

        set => timeZone = value;
    }

    /// <summary>
    /// Get the number of times the <see cref="ICustomCalendarTrigger" /> has already fired.
    /// </summary>
    public int TimesTriggered { get; set; }

    /// <summary>
    /// Validates the misfire instruction.
    /// </summary>
    /// <param name="misfireInstruction">The misfire instruction.</param>
    /// <returns></returns>
    protected override bool ValidateMisfireInstruction(int misfireInstruction)
    {
        if (misfireInstruction < Quartz.MisfireInstruction.IgnoreMisfirePolicy)
        {
            return false;
        }

        if (misfireInstruction > Quartz.MisfireInstruction.CustomCalendarTrigger.DoNothing)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Updates the <see cref="ICustomCalendarTrigger" />'s state based on the
    /// MisfireInstruction.XXX that was selected when the <see cref="ICustomCalendarTrigger" />
    /// was created.
    /// </summary>
    /// <remarks>
    /// If the misfire instruction is set to <see cref="MisfireInstruction.SmartPolicy" />,
    /// then the following scheme will be used:
    /// <ul>
    ///     <li>The instruction will be interpreted as <see cref="MisfireInstruction.CustomCalendarTrigger.FireOnceNow" /></li>
    /// </ul>
    /// </remarks>
    public override void UpdateAfterMisfire(ICalendar? cal)
    {
        int instr = MisfireInstruction;

        if (instr == Quartz.MisfireInstruction.IgnoreMisfirePolicy)
        {
            return;
        }

        if (instr == Quartz.MisfireInstruction.SmartPolicy)
        {
            instr = Quartz.MisfireInstruction.CustomCalendarTrigger.FireOnceNow;
        }

        if (instr == Quartz.MisfireInstruction.CustomCalendarTrigger.DoNothing)
        {
            DateTimeOffset? newFireTime = GetFireTimeAfter(TimeProvider.GetUtcNow());
            while (newFireTime != null && cal != null && !cal.IsTimeIncluded(newFireTime.Value))
            {
                newFireTime = GetFireTimeAfter(newFireTime);
            }
            SetNextFireTimeUtc(newFireTime);
        }
        else if (instr == Quartz.MisfireInstruction.CustomCalendarTrigger.FireOnceNow)
        {
            // fire once now...
            SetNextFireTimeUtc(TimeProvider.GetUtcNow());
            // the new fire time afterward will magically preserve the original
            // time of day for firing for day/week/month interval triggers,
            // because of the way getFireTimeAfter() works - in its always restarting
            // computation from the start time.
        }
    }

    /// <summary>
    /// This method should not be used by the Quartz client.
    /// <para>
    /// Called when the <see cref="IScheduler" /> has decided to 'fire'
    /// the trigger (Execute the associated <see cref="IJob" />), in order to
    /// give the <see cref="ITrigger" /> a chance to update itself for its next
    /// triggering (if any).
    /// </para>
    /// </summary>
    /// <seealso cref="JobExecutionException" />
    public override void Triggered(ICalendar? calendar)
    {
        TimesTriggered++;
        previousFireTimeUtc = nextFireTimeUtc;
        nextFireTimeUtc = GetFireTimeAfter(nextFireTimeUtc);

        while (nextFireTimeUtc != null && calendar != null
                                       && !calendar.IsTimeIncluded(nextFireTimeUtc.Value))
        {
            nextFireTimeUtc = GetFireTimeAfter(nextFireTimeUtc);
        }
    }

    /// <summary>
    /// This method should not be used by the Quartz client.
    /// <para>
    /// The implementation should update the <see cref="ITrigger" />'s state
    /// based on the given new version of the associated <see cref="ICalendar" />
    /// (the state should be updated so that it's next fire time is appropriate
    /// given the Calendar's new settings).
    /// </para>
    /// </summary>
    /// <param name="calendar"> </param>
    /// <param name="misfireThreshold"></param>
    public override void UpdateWithNewCalendar(ICalendar calendar, TimeSpan misfireThreshold)
    {
        nextFireTimeUtc = GetFireTimeAfter(previousFireTimeUtc);

        if (nextFireTimeUtc == null || calendar == null)
        {
            return;
        }

        DateTimeOffset now = TimeProvider.GetUtcNow();
        while (nextFireTimeUtc != null && !calendar.IsTimeIncluded(nextFireTimeUtc.Value))
        {
            nextFireTimeUtc = GetFireTimeAfter(nextFireTimeUtc);

            if (nextFireTimeUtc == null)
            {
                break;
            }

            //avoid infinite loop
            if (nextFireTimeUtc.Value.Year > TriggerConstants.YearToGiveUpSchedulingAt)
            {
                nextFireTimeUtc = null;
            }

            if (nextFireTimeUtc != null && nextFireTimeUtc < now)
            {
                TimeSpan diff = now - nextFireTimeUtc.Value;
                if (diff >= misfireThreshold)
                {
                    nextFireTimeUtc = GetFireTimeAfter(nextFireTimeUtc);
                }
            }
        }
    }

    /// <summary>
    /// Called by the scheduler at the time a <see cref="ITrigger" /> is first
    /// added to the scheduler, in order to have the <see cref="ITrigger" />
    /// compute its first fire time, based on any associated calendar.
    /// <para>
    /// After this method has been called, <see cref="GetNextFireTimeUtc" />
    /// should return a valid answer.
    /// </para>
    /// </summary>
    /// <param name="calendar"></param>
    /// <returns>
    /// the first time at which the <see cref="ITrigger" /> will be fired
    /// by the scheduler, which is also the same value <see cref="GetNextFireTimeUtc" />
    /// will return (until after the first firing of the <see cref="ITrigger" />).
    /// </returns>
    public override DateTimeOffset? ComputeFirstFireTimeUtc(ICalendar? calendar)
    {
        nextFireTimeUtc = GetFireTimeAfter(startTimeUtc.AddSeconds(-1));

        // Check calendar for date-time exclusion
        while (nextFireTimeUtc != null && calendar != null
                                       && !calendar.IsTimeIncluded(nextFireTimeUtc.Value))
        {
            nextFireTimeUtc = GetFireTimeAfter(nextFireTimeUtc);
        }

        return nextFireTimeUtc;
    }

    /// <summary>
    /// Returns the next time at which the <see cref="ITrigger" /> is scheduled to fire. If
    /// the trigger will not fire again, <see langword="null" /> will be returned.  Note that
    /// the time returned can possibly be in the past, if the time that was computed
    /// for the trigger to next fire has already arrived, but the scheduler has not yet
    /// been able to fire the trigger (which would likely be due to lack of resources
    /// e.g. threads).
    /// </summary>
    ///<remarks>
    /// The value returned is not guaranteed to be valid until after the <see cref="ITrigger" />
    /// has been added to the scheduler.
    /// </remarks>
    /// <returns></returns>
    public override DateTimeOffset? GetNextFireTimeUtc()
    {
        return nextFireTimeUtc;
    }

    /// <summary>
    /// Returns the previous time at which the <see cref="ICustomCalendarTrigger" /> fired.
    /// If the trigger has not yet fired, <see langword="null" /> will be returned.
    /// </summary>
    public override DateTimeOffset? GetPreviousFireTimeUtc()
    {
        return previousFireTimeUtc;
    }

    public override void SetNextFireTimeUtc(DateTimeOffset? value)
    {
        nextFireTimeUtc = value;
    }

    public override void SetPreviousFireTimeUtc(DateTimeOffset? previousFireTimeUtc)
    {
        this.previousFireTimeUtc = previousFireTimeUtc;
    }

    /// <summary>
    /// Returns the next time at which the <see cref="ICustomCalendarTrigger" /> will fire,
    /// after the given time. If the trigger will not fire after the given time,
    /// <see langword="null" /> will be returned.
    /// </summary>
    public override DateTimeOffset? GetFireTimeAfter(DateTimeOffset? afterTimeUtc)
    {
        // Check repeatCount limit
        if (repeatCount != RepeatIndefinitely && (TimesTriggered > repeatCount || repeatCount == 0))
        {
            return null;
        }

        // set afterTimeUtc to now if null
        if (!afterTimeUtc.HasValue)
        {
            afterTimeUtc = TimeProvider.GetUtcNow();
        }

        // return null if afterTimeUtc is greater than endTimeUtc
        if (EndTimeUtc.HasValue && afterTimeUtc.Value.CompareTo(EndTimeUtc.Value) >= 0)
        {
            return null;
        }

        DateTimeOffset? pot = GetTimeAfter(afterTimeUtc.Value);
        if (EndTimeUtc.HasValue && pot.HasValue && pot.Value > EndTimeUtc.Value)
        {
            return null;
        }

        return pot;
    }

    // 1. Dayly
    // FREQ=DAILY;INTERVAL=5;UNTIL=20240509T160000Z
    // FREQ=DAILY;INTERVAL=5;COUNT=5

    // 2. Weekly
    // FREQ=WEEKLY;BYDAY=SU,WE,TH,SA;INTERVAL=3;COUNT=6

    // 3. Monthly
    // FREQ=MONTHLY;BYMONTHDAY=5,9,31;INTERVAL=6;UNTIL=20240509T160000Z
    // FREQ=MONTHLY;BYDAY=WE,SU,SA;INTERVAL=6;UNTIL=20240509T160000Z

    // 4. Yearly
    // FREQ=YEARLY;INTERVAL=3;BYMONTH=3;BYDAY=1MO,5FR,-1WE
    // FREQ=YEARLY;INTERVAL=3;BYMONTH=4;BYMONTHDAY=1,3,30,31
    private string? ConstructRecurrencePatternStrings()
    {
        string? recurrencePatterns;

        switch (RepeatIntervalUnit)
        {
            case IntervalUnit.Day:
                recurrencePatterns = $"FREQ=DAILY;INTERVAL={RepeatInterval}";
                break;
            case IntervalUnit.Week:
                recurrencePatterns = $"FREQ=WEEKLY;INTERVAL={RepeatInterval};BYDAY={ByDay}";
                break;
            case IntervalUnit.Month:
                {
                    recurrencePatterns = $"FREQ=MONTHLY;INTERVAL={RepeatInterval}";
                    if (!string.IsNullOrEmpty(ByMonthDay))
                    {
                        recurrencePatterns += $";BYMONTHDAY={ByMonthDay}";
                    }
                    else if (!string.IsNullOrEmpty(byDay))
                    {
                        recurrencePatterns += $";BYDAY={ByDay}";
                    }
                }
                break;
            case IntervalUnit.Year:
                {
                    recurrencePatterns = $"FREQ=YEARLY;INTERVAL={RepeatInterval};BYMONTH={ByMonth}";
                    if (!string.IsNullOrEmpty(ByMonthDay))
                    {
                        recurrencePatterns += $";BYMONTHDAY={ByMonthDay}";
                    }
                    else if (!string.IsNullOrEmpty(byDay))
                    {
                        recurrencePatterns += $";BYDAY={ByDay}";
                    }
                }
                break;
            default:
                throw new NotSupportedException($"{RepeatIntervalUnit} is not supported.");
        }

        recurrencePatterns += $";COUNT=500";

        return recurrencePatterns;
    }

    /// <summary>
    /// Recalculate start time to minimize the evaluator loop
    /// </summary>
    /// <param name="afterTimeUtc"></param>
    /// <returns></returns>
    private DateTimeOffset ReCalculateStartTimeUtc(DateTimeOffset afterTimeUtc)
    {
        var newStartTimeUtc = startTimeUtc;

        // Dictionary to map IntervalUnit to corresponding add function
        var intervalActions = new Dictionary<IntervalUnit, Func<DateTimeOffset, DateTimeOffset>>()
        {
            { IntervalUnit.Year, dt => dt.AddYears(1 * RepeatInterval) },
            { IntervalUnit.Month, dt => dt.AddMonths(1 * RepeatInterval) },
            { IntervalUnit.Week, dt => dt.AddDays(7 * RepeatInterval) },
            { IntervalUnit.Day, dt => dt.AddDays(1 * RepeatInterval) } // Assuming default case is adding days
        };

        // Get the appropriate add function based on the RepeatIntervalUnit
        var addInterval = intervalActions.ContainsKey(RepeatIntervalUnit) ? intervalActions[RepeatIntervalUnit] : intervalActions[IntervalUnit.Day];

        while (newStartTimeUtc < afterTimeUtc)
        {
            var tempDateTimeUtc = addInterval(newStartTimeUtc);
            if (tempDateTimeUtc >= afterTimeUtc) break;
            newStartTimeUtc = tempDateTimeUtc;
        }

        return newStartTimeUtc;
    }


    /// <summary>
    /// Gets the next time to fire after the given time.
    /// </summary>
    /// <param name="afterTimeUtc">The UTC time to compute from.</param>
    /// <returns></returns>
    private DateTimeOffset? GetTimeAfter(DateTimeOffset afterTimeUtc)
    {
        try
        {
            string? patterns = ConstructRecurrencePatternStrings();
            RecurrencePatternEvaluator evaluator = new(new RecurrencePattern(patterns));

            DateTimeOffset sTime = ReCalculateStartTimeUtc(afterTimeUtc);
            DateTimeOffset eTime = EndTimeUtc != null ? EndTimeUtc.Value : DateTimeOffset.MaxValue.UtcDateTime;
            if (timeZone != null)
            {
                sTime = TimeZoneInfo.ConvertTimeFromUtc(sTime.UtcDateTime, timeZone);
                eTime = TimeZoneInfo.ConvertTimeFromUtc(eTime.UtcDateTime, timeZone);
            }
            else
            {
                timeZone = TimeZoneInfo.Utc;
            }

            DateTime start = sTime.DateTime;
            DateTime end = eTime.DateTime;

            HashSet<Period> occurrences = evaluator.Evaluate(new CalDateTime(start, timeZone.Id), start, end, includeReferenceDateInResults: false);
            if (occurrences == null || occurrences.Count <= 0)
            {
                return null;
            }
        
            Period? occurrence = occurrences.FirstOrDefault(o => o.StartTime.AsUtc > afterTimeUtc);
            if (occurrence == null)
            {
                return null;
            }

            return occurrence.StartTime.AsDateTimeOffset;
        }
        catch (Exception ex)
        {
            ThrowHelper.ThrowSchedulerException(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Returns the final time at which the <see cref="ICustomCalendarTrigger" /> will
    /// fire, if there is no end time set, null will be returned.
    /// </summary>
    /// <value></value>
    /// <remarks>Note that the return time may be in the past.</remarks>
    public override DateTimeOffset? FinalFireTimeUtc
    {
        get
        {
            if (EndTimeUtc == null)
            {
                return null;
            }

            // back up a second from end time
            DateTimeOffset? fTime = EndTimeUtc.Value.AddSeconds(-1);
            // find the next fire time after that
            fTime = GetFireTimeAfter(fTime);

            // the trigger fires at the end time, that's it!
            if (fTime == null || fTime == EndTimeUtc)
            {
                return fTime;
            }

            // otherwise we have to back up one interval from the fire time after the end time

            DateTimeOffset lTime = fTime.Value;

            if (RepeatIntervalUnit == IntervalUnit.Day)
            {
                lTime = lTime.AddDays(-1 * RepeatInterval);
            }
            else if (RepeatIntervalUnit == IntervalUnit.Week)
            {
                lTime = lTime.AddDays(-1 * RepeatInterval * 7);
            }
            else if (RepeatIntervalUnit == IntervalUnit.Month)
            {
                lTime = lTime.AddMonths(-1 * RepeatInterval);
            }
            else if (RepeatIntervalUnit == IntervalUnit.Year)
            {
                lTime = lTime.AddYears(-1 * RepeatInterval);
            }

            return lTime;
        }
    }

    /// <summary>
    /// Determines whether or not the <see cref="ICustomCalendarTrigger" /> will occur
    /// again.
    /// </summary>
    /// <returns></returns>
    public override bool GetMayFireAgain()
    {
        return GetNextFireTimeUtc() != null;
    }

    /// <summary>
    /// Validates whether the properties of the <see cref="IJobDetail" /> are
    /// valid for submission into a <see cref="IScheduler" />.
    /// </summary>
    public override void Validate()
    {
        base.Validate();

        if (RepeatIntervalUnit != IntervalUnit.Week && RepeatIntervalUnit != IntervalUnit.Day
            && RepeatIntervalUnit != IntervalUnit.Month && RepeatIntervalUnit != IntervalUnit.Year)
        {
            ThrowHelper.ThrowSchedulerException("Invalid repeat IntervalUnit (must be Day, Month, Week or Year).");
        }

        if (repeatInterval < 1)
        {
            ThrowHelper.ThrowSchedulerException("Repeat Interval cannot be zero.");
        }

        // FREQ=YEARLY;INTERVAL=2;BYMONTH=1;BYDAY=1MO,1WE
        // FREQ=YEARLY;INTERVAL=3;BYMONTH=3;BYMONTHDAY=1,5,9,30,31
        if (RepeatIntervalUnit == IntervalUnit.Year)
        {
            if (ByMonth < 1 || ByMonth > 12)
            {
                ThrowHelper.ThrowSchedulerException("BYMONTH must be between 1 and 12, but was {ByMonth}.");
            }

            var validByDay = !string.IsNullOrEmpty(ByDay);
            var validByMonthDay = !string.IsNullOrEmpty(ByMonthDay);
            if (!validByDay && !validByMonthDay)
            {
                ThrowHelper.ThrowSchedulerException("By day / By month day must be set correctly, but was {ByDay} / {ByMonthDay}");
            }
        }

        // FREQ=MONTHLY;INTERVAL=2;BYMONTHDAY=1,3,30,31
        // FREQ=MONTHLY;INTERVAL=2;BYDAY=1MO,1FR,-1SU
        if (RepeatIntervalUnit == IntervalUnit.Month)
        {
            var validByDay = !string.IsNullOrEmpty(ByDay);
            var validByMonthDay = !string.IsNullOrEmpty(ByMonthDay);
            if (!validByDay && !validByMonthDay)
            {
                ThrowHelper.ThrowSchedulerException("By day / By month day must be set correctly, but was {ByDay} / {ByMonthDay}");
            }
        }

        // FREQ=WEEKLY;INTERVAL=3;BYDAY=MO,WE
        if (RepeatIntervalUnit == IntervalUnit.Week)
        {
            var validByDay = !string.IsNullOrEmpty(ByDay);
            if (!validByDay)
            {
                ThrowHelper.ThrowSchedulerException("By day must be set correctly, but was {ByDay}");
            }
        }
    }

    public override IScheduleBuilder GetScheduleBuilder()
    {
        CustomCalendarScheduleBuilder cb = CustomCalendarScheduleBuilder.Create()
            .WithInterval(RepeatInterval, RepeatIntervalUnit)
            .InTimeZone(TimeZone)
            .WithRepeatCount(RepeatCount);

        switch (MisfireInstruction)
        {
            case Quartz.MisfireInstruction.CustomCalendarTrigger.DoNothing:
                cb.WithMisfireHandlingInstructionDoNothing();
                break;
            case Quartz.MisfireInstruction.CustomCalendarTrigger.FireOnceNow:
                cb.WithMisfireHandlingInstructionFireAndProceed();
                break;
            case Quartz.MisfireInstruction.IgnoreMisfirePolicy:
                cb.WithMisfireHandlingInstructionIgnoreMisfires();
                break;
        }

        return cb;
    }
}