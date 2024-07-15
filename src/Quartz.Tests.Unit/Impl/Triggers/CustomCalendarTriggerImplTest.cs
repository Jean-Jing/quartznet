using NUnit.Framework;
using Quartz.Impl.Triggers;

namespace Quartz.Tests.Unit.Impl.Triggers;

[TestFixture]
public class CustomCalendarTriggerImplTest
{
    private readonly Random _random;

    public CustomCalendarTriggerImplTest()
    {
        _random = new Random();
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsNull_StartTimeUtcIsMinValue_RepeatCountIsZero_TimesTriggeredEqualsRepeatCount_EndTimeUtcIsNull()
    {
        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = null,
            RepeatCount = 0,
            StartTimeUtc = DateTimeOffset.MinValue,
            RepeatInterval = 1
        };

        DateTimeOffset? afterTimeUtc = null;

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsNull_StartTimeUtcIsBeforeNow_RepeatCountIsZero_TimesTriggeredIsGreaterThanRepeatCount_EndTimeUtcIsNull()
    {
        var startTimeUtc = DateTimeOffset.MinValue.AddDays(_random.Next(1, 10));
        DateTimeOffset? endTimeUtc = null;
        DateTimeOffset? afterTimeUtc = null;

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = endTimeUtc,
            RepeatCount = 0,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 1
        };

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsGreaterThanStartTimeUtc_RepeatCountIsRepeatIndefinitely_EndTimeUtcIsNull()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 5, 0, 0, TimeSpan.Zero);

        DateTimeOffset? endTimeUtc = null;
        var afterTimeUtc = startTimeUtc.AddDays(1);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = endTimeUtc,
            RepeatCount = CustomCalendarTriggerImpl.RepeatIndefinitely,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNotNull(actual);
        Assert.AreEqual(startTimeUtc.AddDays(2), actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsGreaterThanStartTimeUtc_RepeatCountIsRepeatIndefinitely_EndTimeUtcEqualsAfterTimeUtc()
    {
        var startTimeUtc = DateTimeOffset.MinValue.AddDays(_random.Next(1, 10));
        var endTimeUtc = startTimeUtc.AddDays(1);
        var afterTimeUtc = endTimeUtc;

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = endTimeUtc,
            RepeatCount = CustomCalendarTriggerImpl.RepeatIndefinitely,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };


        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsGreaterThanStartTimeUtc_RepeatCountIsRepeatIndefinitely_EndTimeUtcIsGreaterThanAfterTimeUtc()
    {
        var startTimeUtc = DateTimeOffset.MinValue.AddDays(_random.Next(1, 10));
        var afterTimeUtc = startTimeUtc.AddDays(5);
        var endTimeUtc = afterTimeUtc.AddDays(1);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = endTimeUtc,
            RepeatCount = CustomCalendarTriggerImpl.RepeatIndefinitely,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsGreaterThanStartTimeUtc_RepeatCountIsRepeatIndefinitely_EndTimeUtcIsLessThanAfterTimeUtc()
    {
        var startTimeUtc = DateTimeOffset.MinValue.AddDays(_random.Next(1, 10));
        var endTimeUtc = startTimeUtc.AddDays(1);
        var afterTimeUtc = endTimeUtc.AddDays(5);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = endTimeUtc,
            RepeatCount = CustomCalendarTriggerImpl.RepeatIndefinitely,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsGreaterThanStartTimeUtc_RepeatCountIsZero_TimesTriggeredEqualsRepeatCount_EndTimeUtcIsNull()
    {
        var startTimeUtc = DateTimeOffset.MinValue.AddDays(_random.Next(1, 10));
        DateTimeOffset? endTimeUtc = null;
        var afterTimeUtc = startTimeUtc.AddDays(5);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = endTimeUtc,
            RepeatCount = 0,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_EndTimeUtcIsLessThanAfterTimeUtc_RepeatCountIsGreaterThanZero_TimesTriggeredIsLessThanRepeatCount()
    {
        var startTimeUtc = DateTimeOffset.MinValue.AddDays(_random.Next(1, 10));
        var endTimeUtc = startTimeUtc.AddMinutes(5);
        var afterTimeUtc = endTimeUtc.AddDays(1);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddMinutes(5),
            RepeatCount = 1,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsLessThanStartTimeUtc_RepeatCountIsGreaterThanZero_TimesTriggeredIsLessThanRepeatCount_EndTimeUtcIsNull()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 5, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = null,
            RepeatCount = 1,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        DateTimeOffset? afterTimeUtc = startTimeUtc.AddMinutes(-1);

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNotNull(actual);
        Assert.AreEqual(startTimeUtc, actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsLessThanStartTimeUtc_RepeatCountIsGreaterThanZero_TimesTriggeredEqualsRepeatCount_EndTimeUtcIsNull()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 5, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = null,
            RepeatCount = 1,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 1
        };

        DateTimeOffset? afterTimeUtc = startTimeUtc.AddMinutes(-1);

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNotNull(actual);
        Assert.AreEqual(startTimeUtc, actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsLessThanStartTimeUtc_RepeatCountIsGreaterThanZero_TimesTriggeredIsGreaterThanRepeatCount_EndTimeUtcIsNull()
    {
        var startTimeUtc = DateTimeOffset.MinValue.AddDays(_random.Next(1, 10));

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = null,
            RepeatCount = 1,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 2
        };

        DateTimeOffset? afterTimeUtc = startTimeUtc.AddMinutes(-1);

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsGreaterThanStartTimeUtc_RepeatCountIsGreaterThanZero_TimesTriggeredIsLessThanRepeatCount_EndTimeUtcIsNull_CalculatedNumberOfTimesExecutedIsGreaterThanRepeatCount()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 5, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddDays(2),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        DateTimeOffset? afterTimeUtc = startTimeUtc.AddDays(4);

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsGreaterThanStartTimeUtc_RepeatCountIsGreaterThanZero_TimesTriggeredIsLessThanRepeatCount_EndTimeUtcIsNull_CalculatedNumberOfTimesExecutedIsLessThanRepeatCount()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 5, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = null,
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        DateTimeOffset? afterTimeUtc = startTimeUtc.AddMinutes(1);

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNotNull(actual);
        Assert.AreEqual(startTimeUtc.Add(TimeSpan.FromDays(1)), actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsGreaterThanStartTimeUtc_RepeatCountIsGreaterThanZero_TimesTriggeredIsLessThanRepeatCount_EndTimeUtcIsNull_CalculatedNumberOfTimesExecutedEqualsThanRepeatCount()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 5, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddYears(100),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        DateTimeOffset? afterTimeUtc = startTimeUtc.AddDays(1);

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNotNull(actual);
        Assert.AreEqual(startTimeUtc.AddDays(2), actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsGreaterThanStartTimeUtc_RepeatCountIsGreaterThanZero_TimesTriggeredIsLessThanRepeatCount_CalculatedNumberOfTimesExecutedEqualsThanRepeatCount_CalculatedTimeIsGreaterThanEndTimeUtc()
    {
        var startTimeUtc = DateTimeOffset.MinValue.AddDays(_random.Next(1, 10));

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddDays(1).AddMinutes(5),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        DateTimeOffset? afterTimeUtc = startTimeUtc.AddDays(1);

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_AfterTimeUtcIsGreaterThanStartTimeUtc_RepeatCountIsGreaterThanZero_TimesTriggeredIsLessThanRepeatCount_CalculatedNumberOfTimesExecutedEqualsThanRepeatCount_CalculatedTimeEqualsEndTimeUtc()
    {
        var startTimeUtc = DateTimeOffset.MinValue.AddDays(_random.Next(1, 10));

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddDays(2),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            TimesTriggered = 0
        };

        DateTimeOffset? afterTimeUtc = startTimeUtc.AddDays(1);

        var actual = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNull(actual);
    }

    [Test]
    public void GetFireTimeAfter_ByDay_Test()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 5, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddDays(22),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 3,
            RepeatIntervalUnit = IntervalUnit.Day,
            TimesTriggered = 0
        };

        // First Fire Time should be the same as the start time
        var firstTimeUtc = trigger.ComputeFirstFireTimeUtc(null);
        Assert.AreEqual(startTimeUtc.UtcDateTime.ToString(), firstTimeUtc.Value.UtcDateTime.ToString());

        // The next fire time should be 3 days after the first fire time
        var nextFireTime = trigger.GetFireTimeAfter(firstTimeUtc);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(firstTimeUtc.Value.AddDays(3), nextFireTime);

        // The next fire time should be 3 days after the previous fire time
        var afterTimeUtc = nextFireTime.Value.AddSeconds(10);
        nextFireTime = trigger.GetFireTimeAfter(afterTimeUtc);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(firstTimeUtc.Value.AddDays(6), nextFireTime);

        // The next fire time should be 3 days after the previous fire time
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(firstTimeUtc.Value.AddDays(9), nextFireTime);
    }

    [Test]
    public void GetFireTimeAfter_ByWeek_Test()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 5, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddDays(22),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            RepeatIntervalUnit = IntervalUnit.Week,
            ByDay = "SU,WE,TH,SA"
        };

        // First Fire Time should be 2024-07-17 = This Wednesday 
        var firstTimeUtc = trigger.ComputeFirstFireTimeUtc(null);
        Assert.AreEqual(firstTimeUtc, startTimeUtc.AddDays(2));

        // The next fire time should 2024-07-18 = This Thursday
        var nextFireTime = trigger.GetFireTimeAfter(firstTimeUtc);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(3));

        // The next fire time should be 2024-07-20 = This Saturday
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(5));

        // The next fire time should be 2024-07-21 = This Sunday
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(6));

        // The next fire time should be 2024-07-24 = Next Wednesday
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(9));
    }

    [Test]
    public void GetFireTimeAfter_ByMonth_ByDay_Test()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 10, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddMonths(6),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 2,
            RepeatIntervalUnit = IntervalUnit.Month,
            ByDay = "WE,FR"
        };

        // First Fire Time should be 2024-07-17 = This Wednesday 
        var firstTimeUtc = trigger.ComputeFirstFireTimeUtc(null);
        Assert.AreEqual(firstTimeUtc, startTimeUtc.AddDays(2));

        // The next fire time should 2024-07-19 = This Friday
        var nextFireTime = trigger.GetFireTimeAfter(firstTimeUtc);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(4));

        // The next fire time should be 2024-07-24 = Next Wednesday
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(9));

        // The next fire time should be 2024-07-26 = Next Friday
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(11));

        // The next fire time should be 2024-07-31 = Next Wednesday
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(16));

        // The next fire time should be 2024-09-04 = Next Wednesday
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(51));
    }

    [Test]
    public void GetFireTimeAfter_ByMonth_ByMonthDay_Normal_Test()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 10, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddMonths(6),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 2,
            RepeatIntervalUnit = IntervalUnit.Month,
            ByMonthDay = "5"
        };

        // First Fire Time should be 2024-09-05
        var firstTimeUtc = trigger.ComputeFirstFireTimeUtc(null);
        Assert.AreEqual(firstTimeUtc, startTimeUtc.AddMonths(2).AddDays(-10));

        // The next fire time should 2024-11-05
        var nextFireTime = trigger.GetFireTimeAfter(firstTimeUtc);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddMonths(4).AddDays(-10));
    }

    [Test]
    public void GetFireTimeAfter_ByMonth_ByMonthDay_Zero_Test()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 10, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddMonths(6),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 2,
            RepeatIntervalUnit = IntervalUnit.Month
        };

        // First Fire Time should be equal to the start time
        var firstTimeUtc = trigger.ComputeFirstFireTimeUtc(null);
        Assert.AreEqual(firstTimeUtc, startTimeUtc);

        // The next fire time should 2024-09-15
        var nextFireTime = trigger.GetFireTimeAfter(firstTimeUtc);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddMonths(2));
    }

    [Test]
    public void GetFireTimeAfter_ByMonth_ByMonthDay_1st_Test()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 10, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddMonths(6),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            RepeatIntervalUnit = IntervalUnit.Month,
            ByMonthDay = "1"
        };

        // First Fire Time should be 2024-08-01
        var firstTimeUtc = trigger.ComputeFirstFireTimeUtc(null);
        Assert.AreEqual(firstTimeUtc, startTimeUtc.AddMonths(1).AddDays(-14));

        // The next fire time should 2024-09-01
        var nextFireTime = trigger.GetFireTimeAfter(firstTimeUtc);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddMonths(2).AddDays(-14));
    }

    [Test]
    public void GetFireTimeAfter_ByMonth_ByMonthDay_31st_Test()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 10, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddMonths(6),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            RepeatIntervalUnit = IntervalUnit.Month,
            ByMonthDay = "31"
        };

        // First Fire Time should be 2024-07-31
        var firstTimeUtc = trigger.ComputeFirstFireTimeUtc(null);
        Assert.AreEqual(firstTimeUtc, startTimeUtc.AddDays(16));

        // The next fire time should 2024-08-31
        var nextFireTime = trigger.GetFireTimeAfter(firstTimeUtc);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddMonths(1).AddDays(16));

        // The next fire time should 2024-10-31
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddMonths(3).AddDays(16));
    }

    [Test]
    public void GetFireTimeAfter_ByYear_ByMonth_ByDay_Test()
    {
        var startTimeUtc = new DateTimeOffset(2024, 4, 15, 5, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddYears(6),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            RepeatIntervalUnit = IntervalUnit.Year,
            ByMonth = 5,
            ByDay = "2WE,3FR,5SU,-1MO"
        };

        // First Fire Time should be 2024-05-08 = The second Wednesday 
        var firstTimeUtc = trigger.ComputeFirstFireTimeUtc(null);
        Assert.AreEqual(firstTimeUtc, startTimeUtc.AddDays(23));

        // The next fire time should 2024-05-17 = The Third Friday
        var nextFireTime = trigger.GetFireTimeAfter(firstTimeUtc);

        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(32));

        // The next fire time should be 2024-05-27 = The last Monday
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(42));
    }

    [Test]
    public void GetFireTimeAfter_ByYear_ByMonth_ByMonthDay_Test()
    {
        var startTimeUtc = new DateTimeOffset(2024, 7, 15, 5, 0, 0, TimeSpan.Zero);

        CustomCalendarTriggerImpl trigger = new CustomCalendarTriggerImpl
        {
            EndTimeUtc = startTimeUtc.AddYears(6),
            RepeatCount = 2,
            StartTimeUtc = startTimeUtc,
            RepeatInterval = 1,
            RepeatIntervalUnit = IntervalUnit.Year,
            ByMonth = 7,
            ByMonthDay = "1,2,30,31"
        };

        // First Fire Time should be 2024-07-30 
        var firstTimeUtc = trigger.ComputeFirstFireTimeUtc(null);
        Assert.AreEqual(firstTimeUtc, startTimeUtc.AddDays(15));

        // The next fire time should 2024-07-31
        var nextFireTime = trigger.GetFireTimeAfter(firstTimeUtc);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddDays(16));

        // The next fire time should be 2025-07-01
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddYears(1).AddDays(-14));

        // The next fire time should be 2025-07-02
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddYears(1).AddDays(-13));

        // The next fire time should be 2025-07-30
        nextFireTime = trigger.GetFireTimeAfter(nextFireTime);
        Assert.IsNotNull(nextFireTime);
        Assert.AreEqual(nextFireTime, startTimeUtc.AddYears(1).AddDays(15));
    }
}