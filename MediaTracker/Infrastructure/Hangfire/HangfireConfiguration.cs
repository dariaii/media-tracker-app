using Hangfire;

namespace MediaTracker.Infrastructure.Hangfire
{
    public static class HangfireConfiguration
    {
        public static void ConfigureRecurringJobs()
        {
            RecurringJob.AddOrUpdate<DailyJob>(
                recurringJobId: "daily-job",
                methodCall: job => job.ExecuteAsync(),
                cronExpression: "0 8 * * *",
                options: new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Local
                });
        }
    }
}