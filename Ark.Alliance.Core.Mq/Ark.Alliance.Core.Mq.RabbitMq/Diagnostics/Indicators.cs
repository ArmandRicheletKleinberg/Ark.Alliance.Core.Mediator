

using Ark.Alliance.Core.Diagnostics;

namespace Ark.Alliance.Core.Mq.RabbitMq.Diagnostics;

/// <summary>
/// Diagnostics indicators for RabbitMQ connection health.
/// </summary>
public class Indicators : IndicatorsBase
{
    /// <summary>Current number of active connections.</summary>
    public Indicator<int> ActiveConnections { get; } =
        new(v => v > 0 ? IndicatorStatusEnum.Success : IndicatorStatusEnum.Warning);
}
