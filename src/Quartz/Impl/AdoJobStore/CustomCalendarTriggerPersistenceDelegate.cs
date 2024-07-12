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

using Quartz.Impl.Triggers;
using Quartz.Spi;
using Quartz.Util;

namespace Quartz.Impl.AdoJobStore;

/// <summary>
/// Persist a CustomCalendarTriggerImpl by converting internal fields to and from
/// SimplePropertiesTriggerProperties.
/// </summary>
/// <see cref="CustomCalendarScheduleBuilder"/>
/// <see cref="ICustomCalendarTrigger"/>
public sealed class CustomCalendarTriggerPersistenceDelegate : SimplePropertiesTriggerPersistenceDelegateSupport
{
    public override bool CanHandleTriggerType(IOperableTrigger trigger)
    {
        var customCalendarTriggerImpl = trigger as CustomCalendarTriggerImpl;
        return customCalendarTriggerImpl != null && !customCalendarTriggerImpl.HasAdditionalProperties;
    }

    public override string GetHandledTriggerTypeDiscriminator()
    {
        return AdoConstants.TriggerTypeCustomCalendar;
    }

    protected override SimplePropertiesTriggerProperties GetTriggerProperties(IOperableTrigger trigger)
    {
        CustomCalendarTriggerImpl calTrig = (CustomCalendarTriggerImpl) trigger;

        SimplePropertiesTriggerProperties props = new SimplePropertiesTriggerProperties();

        props.Int1 = calTrig.RepeatInterval;
        props.Int2 = calTrig.TimesTriggered;
        
        props.Long1 = calTrig.RepeatCount;
        props.Long2 = calTrig.ByMonth;

        props.String1 = calTrig.RepeatIntervalUnit.ToString();
        props.String2 = calTrig.ByMonthDay;
        props.String3 = calTrig.ByDay;

        props.TimeZoneId = calTrig.TimeZone.Id;

        return props;
    }

    protected override TriggerPropertyBundle GetTriggerPropertyBundle(SimplePropertiesTriggerProperties props)
    {
        int repeatCount = (int)props.Long1;
        int interval = props.Int1;

        var intervalUnitStr = props.String1;

        IntervalUnit intervalUnit = (IntervalUnit) Enum.Parse(typeof(IntervalUnit), intervalUnitStr!, true);

        CustomCalendarScheduleBuilder sb = CustomCalendarScheduleBuilder.Create()
            .WithInterval(interval, intervalUnit)
            .WithRepeatCount(repeatCount);

        if (!string.IsNullOrEmpty(props.TimeZoneId) && props.TimeZoneId != null)
        {
            sb.InTimeZone(TimeZoneUtil.FindTimeZoneById(props.TimeZoneId));
        }

        if (props.Long2 > 0)
        {
            sb.ByMonth((int)props.Long2);
        }

        if (!string.IsNullOrEmpty(props.String2))
        {
            sb.ByMonthDay(Convert.ToInt32(props.String2));
        }

        if (!string.IsNullOrEmpty(props.String3))
        {
            sb.ByDay(props.String3);
        }

        int timesTriggered = props.Int2;

        string[] statePropertyNames = { "timesTriggered" };
        object[] statePropertyValues = { timesTriggered };

        return new TriggerPropertyBundle(sb, statePropertyNames, statePropertyValues);
    }
}