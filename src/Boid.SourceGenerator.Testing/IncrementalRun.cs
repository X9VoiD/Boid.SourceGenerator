using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace Boid.SourceGenerator.Testing;

public class IncrementalRun
{
    private readonly IVerifier _verifier;
    internal TestState TestState { get; }
    internal IncrementalCompilation PreGeneratorCompilation { get; }
    internal GeneratorDriver Driver { get; }

    internal IncrementalRun(IVerifier verifier, IIncrementalGenerator sourceGen, TestState testState)
        : this(verifier, testState, CreateCompilation(testState), CreateDriver(sourceGen, testState))
    {
    }

    private IncrementalRun(IVerifier verifier, TestState testState, IncrementalCompilation preGeneratorCompilation, GeneratorDriver driver)
    {
        _verifier = verifier;
        TestState = testState;
        PreGeneratorCompilation = preGeneratorCompilation;
        Driver = driver;
    }

    public IncrementalRun RunGenerator()
    {
        var newDriver = Driver.RunGeneratorsAndUpdateCompilation(PreGeneratorCompilation.Compilation, out var outputCompilation, out var diagnostics);

        var runResult = newDriver.GetRunResult();
        var generatorDiagnostics = runResult.Results.SelectMany(r => r.Diagnostics);
        var compilationDiagnostics = outputCompilation.GetDiagnostics();

        _verifier.VerifyGeneratedSources(TestState, runResult);
        _verifier.VerifyZeroDiagnostics(TestState, generatorDiagnostics, "generator", TestBehaviour.SkipGeneratorDiagnostic);
        _verifier.VerifyZeroDiagnostics(TestState, compilationDiagnostics, "compilation", TestBehaviour.SkipCompilationDiagnostic);

        // return a new run with the updated driver
        return new IncrementalRun(_verifier, TestState, PreGeneratorCompilation, newDriver);
    }

    internal IncrementalRun UpdateRun(TestState testState)
    {
        // TODO: update incremental compilation
        // TODO: update additional texts
        // TODO: update analyzer config options

        throw new NotImplementedException();
    }

    private static IncrementalCompilation CreateCompilation(TestState testState)
    {
        var incrementalCompilation = new IncrementalCompilation();

        foreach (var sourceFile in testState.Sources)
        {
            incrementalCompilation = incrementalCompilation.SetSource(sourceFile.HintPath, sourceFile.Content);
        }

        return incrementalCompilation;
    }

    private static GeneratorDriver CreateDriver(IIncrementalGenerator sourceGen, TestState testState)
    {
        var driver = CSharpGeneratorDriver.Create(sourceGen)
            .WithUpdatedParseOptions(CSharpParseOptions.Default.WithLanguageVersion(testState.LanguageVersion))
            .WithUpdatedAnalyzerConfigOptions(new AnalyzerConfigOptionsProvider(testState.AnalyzerConfigOptions, testState.AnalyzerConfigOptionsFactory))
            .AddAdditionalTexts(testState.AdditionalText.Select(text =>
                (Microsoft.CodeAnalysis.AdditionalText)new AdditionalText(text.HintPath, text.Content)).ToImmutableArray());

        return driver;
    }
}