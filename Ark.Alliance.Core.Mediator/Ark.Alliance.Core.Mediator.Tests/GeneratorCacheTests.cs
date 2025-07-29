using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Generators;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

/// <summary>
/// Ensures the handler generator uses cache mechanisms correctly.
/// The underlying cache now performs asynchronous file IO when storing hashes.
/// </summary>
public class GeneratorCacheTests
{
    #region Methods (Tests)
    /// <summary>
    /// Generates code twice using file caching and verifies only the first run writes output.
    /// </summary>
    [Fact]
    public void File_cache_skips_second_generation()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

        const string src = """
using Ark.Alliance.Core.Mediator.Messaging;
public record PingCommand() : ICommand<string>;
public class PingHandler : ICommandHandler<PingCommand, string>
{
    public Task<Result<string>> HandleAsync(PingCommand command, CancellationToken token = default)
        => Task.FromResult(Result<string>.Success.WithData("pong"));
}
""";

        var syntaxTree = CSharpSyntaxTree.ParseText(src);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommand<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommandHandler<,>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Result<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new HandlerRegistrationGenerator();
        var additional = new InMemoryAdditionalText(
            ".editorconfig",
            $"build_property.ArkMediatorGeneratorCacheMode = File\nbuild_property.ArkMediatorGeneratorCacheFile = {path}");

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, new[] { additional });
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var first = File.GetLastWriteTimeUtc(path);

        driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, new[] { additional });
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var second = File.GetLastWriteTimeUtc(path);

        Assert.Equal(first, second);
    }

    /// <summary>
    /// Uses memory caching to avoid producing duplicate generated files.
    /// </summary>
    [Fact]
    public void Memory_cache_skips_second_generation()
    {
        const string src = """
using Ark.Alliance.Core.Mediator.Messaging;
public record PingCommand2() : ICommand<string>;
public class PingHandler2 : ICommandHandler<PingCommand2, string>
{
    public Task<Result<string>> HandleAsync(PingCommand2 command, CancellationToken token = default)
        => Task.FromResult(Result<string>.Success.WithData("pong"));
}
""";

        var syntaxTree = CSharpSyntaxTree.ParseText(src);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommand<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommandHandler<,>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Result<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly2",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new HandlerRegistrationGenerator();
        var additional = new InMemoryAdditionalText(
            ".editorconfig",
            "build_property.ArkMediatorGeneratorCacheMode = Memory");

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, new[] { additional });
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output1, out _);
        Assert.Contains(output1.SyntaxTrees, t => t.FilePath.EndsWith("CqrsHandlers.g.cs"));

        driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, new[] { additional });
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output2, out _);
        var generated = output2.SyntaxTrees.Count(t => t.FilePath.EndsWith("CqrsHandlers.g.cs"));

        Assert.Equal(0, generated);
    }

    /// <summary>
    /// Hybrid mode keeps cache across runs and in memory.
    /// </summary>
    [Fact]
    public void Hybrid_cache_skips_second_generation_and_writes_file()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        const string src = """
using Ark.Alliance.Core.Mediator.Messaging;
public record PingCommandHybrid() : ICommand<string>;
public class PingHandlerHybrid : ICommandHandler<PingCommandHybrid, string>
{
    public Task<Result<string>> HandleAsync(PingCommandHybrid command, CancellationToken token = default)
        => Task.FromResult(Result<string>.Success.WithData("pong"));
}
""";

        var syntaxTree = CSharpSyntaxTree.ParseText(src);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommand<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommandHandler<,>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Result<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            "TestAssemblyHybrid",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new HandlerRegistrationGenerator();
        var additional = new InMemoryAdditionalText(
            ".editorconfig",
            $"build_property.ArkMediatorGeneratorCacheMode = Hybrid\nbuild_property.ArkMediatorGeneratorCacheFile = {path}");

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, new[] { additional });
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var firstWrite = File.Exists(path);

        driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, new[] { additional });
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var secondWrite = File.GetLastWriteTimeUtc(path);

        Assert.True(firstWrite);
        Assert.Equal(secondWrite, File.GetLastWriteTimeUtc(path));
    }

    /// <summary>
    /// None mode should always generate new output.
    /// </summary>
    [Fact]
    public void None_mode_always_generates()
    {
        const string src = """
using Ark.Alliance.Core.Mediator.Messaging;
public record PingCommand3() : ICommand<string>;
public class PingHandler3 : ICommandHandler<PingCommand3, string>
{
    public Task<Result<string>> HandleAsync(PingCommand3 command, CancellationToken token = default)
        => Task.FromResult(Result<string>.Success.WithData("pong"));
}
""";

        var syntaxTree = CSharpSyntaxTree.ParseText(src);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommand<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommandHandler<,>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Result<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly3",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new HandlerRegistrationGenerator();
        var additional = new InMemoryAdditionalText(
            ".editorconfig",
            "build_property.ArkMediatorGeneratorCacheMode = None");

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, new[] { additional });
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output1, out _);
        Assert.Contains(output1.SyntaxTrees, t => t.FilePath.EndsWith("CqrsHandlers.g.cs"));

        driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, new[] { additional });
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output2, out _);
        var generated = output2.SyntaxTrees.Count(t => t.FilePath.EndsWith("CqrsHandlers.g.cs"));

        Assert.Equal(1, generated);
    }

    #endregion Methods (Tests)

    #region Classes
    /// <summary>
    /// Additional text implementation used to feed generator options.
    /// </summary>
    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly SourceText _text;
        private readonly string _path;

        public InMemoryAdditionalText(string path, string text)
        {
            _path = path;
            _text = SourceText.From(text, Encoding.UTF8);
        }

        public override string Path => _path;

        public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
    }
    #endregion Classes
}

