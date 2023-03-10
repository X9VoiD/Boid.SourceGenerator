using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace Boid.SourceGenerator.Testing;

public partial class IncrementalGeneratorVerifier<TSourceGenerator, TVerifier>
    where TSourceGenerator : IIncrementalGenerator, new()
    where TVerifier : IVerifier, new()
{
    private readonly TVerifier _verifier = new();

    public TestState TestState { get; set; } = new();

    public ImmutableArray<MetadataReference> AdditionalReferences { get; init; } = ImmutableArray<MetadataReference>.Empty;
    public ImmutableArray<IncrementalRunObserver> Observers { get; init; } = ImmutableArray<IncrementalRunObserver>.Empty;

    public Task RunAsync()
    {
        // capture state to avoid potentially referring to different instances during the same run
        var testState = TestState;

        return Task.Run(() =>
        {
            var sourceGen = CreateGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(CreateGenerator())
                .WithUpdatedParseOptions(CSharpParseOptions.Default.WithLanguageVersion(testState.LanguageVersion))
                .WithUpdatedAnalyzerConfigOptions(new AnalyzerConfigOptionsProvider(testState.AnalyzerConfigOptions, testState.AnalyzerConfigOptionsFactory))
                .AddAdditionalTexts(testState.AdditionalText.Select(text =>
                    (Microsoft.CodeAnalysis.AdditionalText)new AdditionalText(text.HintPath, text.Content)).ToImmutableArray());

            var compilation = CreateCompilation();
            Observers.ForEach(o => o.OnRunStartInternal(0));
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            var runResult = driver.GetRunResult();
            var generatorDiagnostics = runResult.Results.SelectMany(r => r.Diagnostics);
            var compilationDiagnostics = outputCompilation.GetDiagnostics();

            IncrementalRunObserver.UnregisterGenerator(sourceGen);

            _verifier.VerifyGeneratedSources(testState, runResult);
            _verifier.VerifyZeroDiagnostics(testState, generatorDiagnostics, "generator", TestBehaviour.SkipGeneratorDiagnostic);
            _verifier.VerifyZeroDiagnostics(testState, compilationDiagnostics, "compilation", TestBehaviour.SkipCompilationDiagnostic);
            _verifier.VerifyObservers(testState, Observers);
        });
    }

    public Task<IncrementalRun> RunIncrementalAsync(IncrementalRun? incrementalRun = null)
    {
        incrementalRun ??= new IncrementalRun(_verifier, new TSourceGenerator(), TestState, AdditionalReferences, Observers);
        return Task.Run(() =>
        {
            IncrementalRunObserver.RegisterGenerator(incrementalRun.Generator, incrementalRun.Observers);
            var newIncrementalRun = incrementalRun.RunGenerator();
            IncrementalRunObserver.UnregisterGenerator(incrementalRun.Generator);
            return newIncrementalRun;
        });
    }

    public async Task RunIncrementalAsync(params TestState[] incrementalStates)
    {
        ArgumentNullException.ThrowIfNull(incrementalStates);
        TestState = incrementalStates[0];

        IncrementalRun? incrementalRun = null;

        foreach (var state in incrementalStates)
        {
            incrementalRun = await RunIncrementalAsync(incrementalRun?.ApplyIncrementalChange(state)).ConfigureAwait(false);
        }
    }

    private Compilation CreateCompilation()
    {
        return CSharpCompilation.Create("test",
            references: AdditionalReferences.Append(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
        .AddSyntaxTrees(TestState.Sources.Select(source => CSharpSyntaxTree.ParseText(source.Content, path: source.HintPath)));
    }

    private TSourceGenerator CreateGenerator()
    {
        var generator = new TSourceGenerator();
        IncrementalRunObserver.RegisterGenerator(generator, Observers);
        return generator;
    }
}