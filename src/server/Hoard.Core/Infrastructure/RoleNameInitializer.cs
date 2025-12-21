using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Hoard.Core.Infrastructure;

public class RoleNameInitializer : ITelemetryInitializer
{
    private readonly string _roleName;

    public RoleNameInitializer(string roleName) => _roleName = roleName;

    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = _roleName;
    }
}