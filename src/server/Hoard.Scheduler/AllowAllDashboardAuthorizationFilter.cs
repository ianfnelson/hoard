using Hangfire.Dashboard;

namespace Hoard.Scheduler;

public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}