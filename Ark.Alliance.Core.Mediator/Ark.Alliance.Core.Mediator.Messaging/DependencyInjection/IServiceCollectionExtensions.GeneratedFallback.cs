using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Ark.Alliance.Core.Mediator.Messaging;

public static partial class IServiceCollectionExtensions
{
    static partial void RegisterGeneratedHandlers(IServiceCollection services)
    {
        // Fallback when source generator output is unavailable.
        var assembly = Assembly.GetCallingAssembly();
        RegisterHandlers(services, assembly, new ArkMessagingOptions());
    }
}
