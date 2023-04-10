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

namespace Quartz.Examples.Example06
{
    /// <summary>
    /// A job dumb job that will throw a job execution exception.
    /// </summary>
    /// <author>Bill Kratzer</author>
    /// <author>Marko Lahma (.NET)</author>
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class BadJob1 : IJob
    {
        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a Trigger" />
        /// fires that is associated with the <see cref="IJob" />.
        /// </summary>
        public virtual ValueTask Execute(IJobExecutionContext context)
        {
            JobKey jobKey = context.JobDetail.Key;
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            int denominator = dataMap.GetInt("denominator");
            Console.WriteLine("{0} with denominator {1}", "---" + jobKey + " executing at " + DateTime.Now, denominator);

            // a contrived example of an exception that
            // will be generated by this job due to a
            // divide by zero error (only on first run)
            try
            {
                int calculation = 4815/denominator;
            }
            catch (Exception e)
            {
                Console.WriteLine("--- Error in job!");
                JobExecutionException e2 = new JobExecutionException(e);

                // fix denominator so the next time this job run
                // it won't fail again
                dataMap.Put("denominator", "1");

                // this job will refire immediately
                e2.RefireImmediately = true;
                throw e2;
            }

            Console.WriteLine("---{0} completed at {1:r}", jobKey, DateTime.Now);
            return default;
        }
    }
}