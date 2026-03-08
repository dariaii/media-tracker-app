using Hangfire;

namespace MediaTracker.Infrastructure.Hangfire
{
    public static class HangfireConfiguration
    {
        public static void ConfigureRecurringJobs()
        {
            RecurringJob.AddOrUpdate<MorningUpdatesJob>(
                recurringJobId: "morning-update",
                methodCall: job => job.ExecuteAsync(),
                cronExpression: "0 8 * * *",
                options: new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Local,
                    MisfireHandling = MisfireHandlingMode.Ignorable
                });
        }
    }
}