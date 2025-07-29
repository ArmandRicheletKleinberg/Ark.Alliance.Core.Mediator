using Ark.Alliance.Core.Mediator.Generators.Hybrid;
using Ark.Alliance.Core.Mediator.Generators.Runtime;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;

public class HybridGenerationTests
{
    [Fact]
    public void Hybrid_registration_adds_services()
    {
        var services = new ServiceCollection();
        services.AddHybridHandlers(Assembly.GetExecutingAssembly());
        Assert.NotEmpty(services);
    }

    [Fact]
    public async Task RuntimeGenerator_returns_code()
    {
        using var gen = new RuntimeGenerator();
        var code = await gen.GenerateHandlerRegistrationsAsync(Assembly.GetExecutingAssembly());
        Assert.NotNull(code);
    }
}
