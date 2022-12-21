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

    public Task RunAsync()
    {
        // capture state to avoid potentially referring to different instances during the same run
        var testState = TestState;

        return Task.Run(() =>
        {
            GeneratorDriver driver = CSharpGeneratorDriver.Create(new TSourceGenerator())
                .WithUpdatedParseOptions(CSharpParseOptions.Default.WithLanguageVersion(testState.LanguageVersion))
                .WithUpdatedAnalyzerConfigOptions(new AnalyzerConfigOptionsProvider(testState.AnalyzerConfigOptions, testState.AnalyzerConfigOptionsFactory))
                .AddAdditionalTexts(testState.AdditionalText.Select(text =>
                    (Microsoft.CodeAnalysis.AdditionalText)new AdditionalText(text.HintPath, text.Content)).ToImmutableArray());

            var compilation = CreateCompilation();
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            var runResult = driver.GetRunResult();
            var generatorDiagnostics = runResult.Results.SelectMany(r => r.Diagnostics);
            var compilationDiagnostics = outputCompilation.GetDiagnostics();

            _verifier.VerifyGeneratedSources(testState, runResult);
            _verifier.VerifyZeroDiagnostics(testState, generatorDiagnostics, "generator", TestBehaviour.SkipGeneratorDiagnostic);
            _verifier.VerifyZeroDiagnostics(testState, compilationDiagnostics, "compilation", TestBehaviour.SkipCompilationDiagnostic);
        });
    }

    public Task<IncrementalRun> RunIncrementalAsync(IncrementalRun? incrementalRun = null)
    {
        incrementalRun ??= new IncrementalRun(_verifier, new TSourceGenerator(), TestState);
        return Task.Run(() => incrementalRun.RunGenerator());
    }

    public async Task RunIncrementalAsync(params TestState[] incrementalStates)
    {
        TestState = incrementalStates[0];

        IncrementalRun? incrementalRun = null;

        foreach (var phase in incrementalStates)
        {
            incrementalRun = await RunIncrementalAsync(incrementalRun?.UpdateRun(phase));
        }
    }

    private Compilation CreateCompilation()
    {
        return CSharpCompilation.Create("test",
            references: AdditionalReferences.Append(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
        .AddSyntaxTrees(TestState.Sources.Select(source => CSharpSyntaxTree.ParseText(source.Content, path: source.HintPath)));
    }
}