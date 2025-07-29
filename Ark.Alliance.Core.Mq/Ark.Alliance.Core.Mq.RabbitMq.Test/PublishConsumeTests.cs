using Ark.Alliance.Core.Mq.RabbitMq;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Resilience;
using Xunit;
using Xunit.Abstractions;

public class PublishConsumeTests : IAsyncLifetime
{
    private readonly IContainer _rabbitContainer;
    private ServiceProvider _provider = null!;
    private readonly ITestOutputHelper _output;

    public PublishConsumeTests(ITestOutputHelper output)
    {
        _output = output;
        _rabbitContainer = new DotNet.Testcontainers.Builders.ContainerBuilder()
            .WithImage("rabbitmq:3-management")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _rabbitContainer.StartAsync();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Failed to start RabbitMQ container: {ex.Message}");
            var logs = await _rabbitContainer.GetLogsAsync();
            _output.WriteLine(logs.Stdout);
            _output.WriteLine("Ensure Docker is running and ports 5672/15672 are available.");
            _output.WriteLine("See _SDLC/04_Testing/testcontainers_troubleshooting.md for guidance.");
            throw;
        }

        var services = new ServiceCollection();
        services.Configure<RabbitMqSettings>(opts =>
        {
            opts.HostName = "localhost";
            opts.Port = _rabbitContainer.GetMappedPublicPort(5672);
            opts.PublisherConfirms = true;
            opts.QueueName = "test.q";
        });

        services.AddLogging();
        services.AddSingleton(new ResiliencePipelineProvider(3));
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IChannelPool, RabbitMqChannelPool>();
        services.AddTransient<RabbitMqPublisher>();
        services.AddTransient<RabbitMqConsumer>();

        _provider = services.BuildServiceProvider();

        // Declare queue for tests
        var pool = (RabbitMqChannelPool)_provider.GetRequiredService<IChannelPool>();
        await using var lease = await pool.AcquireAsync();
        await lease.Channel.QueueDeclareAsync("test.q", true, false, false);
    }

    public async Task DisposeAsync()
    {
        try
        {
            await _rabbitContainer.StopAsync();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Failed to stop RabbitMQ container: {ex.Message}");
            _output.WriteLine("Container shutdown issues may leave resources running. Use 'docker ps -a' to inspect.");
        }
        _provider.Dispose();
    }

    [DockerAvailableFact]
    public async Task Publish_and_consume_message()
    {
        try
        {
            var publisher = _provider.GetRequiredService<RabbitMqPublisher>();
            var consumer = _provider.GetRequiredService<RabbitMqConsumer>();
            bool received = false;

            await consumer.ConsumeAsync<string>("test.q", msg => { received = msg == "hello"; return Task.CompletedTask; });

            await publisher.PublishAsync("test.q", "test.q", "hello");

            await Task.Delay(1000);
            Assert.True(received);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"RabbitMQ integration test failed: {ex.Message}");
            _output.WriteLine("Consult testcontainers_troubleshooting.md for steps to diagnose container issues.");
            throw;
        }
    }

    [DockerAvailableFact]
    public async Task Channel_pool_respects_size_limit()
    {
        try
        {
            var pool = _provider.GetRequiredService<IChannelPool>();

            var lease1 = await pool.AcquireAsync();
            var cts = new CancellationTokenSource(200);
            var leaseTask = pool.AcquireAsync(cts.Token);

            await Task.Delay(50);
            await lease1.DisposeAsync();

            var lease2 = await leaseTask;
            Assert.NotNull(lease2);
            await lease2.DisposeAsync();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"RabbitMQ channel pool test failed: {ex.Message}");
            _output.WriteLine("If container setup failed see _SDLC/04_Testing/testcontainers_troubleshooting.md.");
            throw;
        }
    }
}
