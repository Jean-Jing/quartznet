using FluentAssertions;

namespace Quartz.Tests.Unit;

[TestFixture]
public class JobBuilderTest
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class TestStatefulJob : IJob
    {
        public ValueTask Execute(IJobExecutionContext context)
        {
            return default;
        }
    }

    public class TestJob : IJob
    {
        public ValueTask Execute(IJobExecutionContext context)
        {
            return default;
        }
    }

    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class TestAnnotatedJob : IJob
    {
        public ValueTask Execute(IJobExecutionContext context)
        {
            return default;
        }
    }

    [SetUp]
    protected void SetUp()
    {
    }

    [Test]
    public void TestJobBuilder()
    {
        IJobDetail job = JobBuilder.Create()
            .OfType<TestJob>()
            .WithIdentity("j1")
            .StoreDurably()
            .Build();

        Assert.AreEqual("j1", job.Key.Name, "Unexpected job name: " + job.Key.Name);
        Assert.IsTrue(job.Key.Group.Equals(JobKey.DefaultGroup), "Unexpected job group: " + job.Key.Group);
        Assert.IsTrue(job.Key.Equals(new JobKey("j1")), "Unexpected job key: " + job.Key);
        Assert.IsTrue(job.Description is null, "Unexpected job description: " + job.Description);
        Assert.IsTrue(job.Durable, "Expected isDurable == true ");
        Assert.IsFalse(job.RequestsRecovery, "Expected requestsRecovery == false ");
        Assert.IsFalse(job.ConcurrentExecutionDisallowed, "Expected isConcurrentExecutionDisallowed == false ");
        Assert.IsFalse(job.PersistJobDataAfterExecution, "Expected isPersistJobDataAfterExecution == false ");
        job.JobType.Type.Should().Be(typeof(TestJob));

        job = JobBuilder.Create()
            .OfType<TestAnnotatedJob>()
            .WithIdentity("j1")
            .WithDescription("my description")
            .StoreDurably(true)
            .RequestRecovery()
            .Build();

        Assert.IsTrue(job.Description.Equals("my description"), "Unexpected job description: " + job.Description);
        Assert.IsTrue(job.Durable, "Expected isDurable == true ");
        Assert.IsTrue(job.RequestsRecovery, "Expected requestsRecovery == true ");
        Assert.IsTrue(job.ConcurrentExecutionDisallowed, "Expected isConcurrentExecutionDisallowed == true ");
        Assert.IsTrue(job.PersistJobDataAfterExecution, "Expected isPersistJobDataAfterExecution == true ");

        job = JobBuilder.Create()
            .OfType<TestStatefulJob>()
            .
            WithIdentity("j1", "g1")
            .RequestRecovery(false)
            .Build();

        Assert.IsTrue(job.Key.Group.Equals("g1"), "Unexpected job group: " + job.Key.Name);
        Assert.IsFalse(job.Durable, "Expected isDurable == false ");
        Assert.IsFalse(job.RequestsRecovery, "Expected requestsRecovery == false ");
        Assert.IsTrue(job.ConcurrentExecutionDisallowed, "Expected isConcurrentExecutionDisallowed == true ");
        Assert.IsTrue(job.PersistJobDataAfterExecution, "Expected isPersistJobDataAfterExecution == true ");
    }
}