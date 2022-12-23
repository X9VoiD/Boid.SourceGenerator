using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace Boid.SourceGenerator.Testing;

public sealed class IncrementalRun
{
    internal int RunNumber { get; }
    internal IVerifier Verifier { get; }
    internal TestState TestState { get; }
    internal IncrementalCompilation PreGeneratorCompilation { get; }
    internal GeneratorDriver Driver { get; }
    internal ImmutableArray<AdditionalText> AdditionalTexts { get; }

    internal IncrementalRun(IVerifier verifier, IIncrementalGenerator sourceGen, TestState testState, ImmutableArray<MetadataReference> references)
    {
        Verifier = verifier;
        AdditionalTexts = CreateAdditionalTexts(testState);
        TestState = testState;
        PreGeneratorCompilation = CreateCompilation(testState, references);
        Driver = CreateDriver(sourceGen, testState, AdditionalTexts);
    }

    private IncrementalRun(
        int runNumber,
        IVerifier verifier,
        TestState testState,
        IncrementalCompilation preGeneratorCompilation,
        GeneratorDriver driver,
        ImmutableArray<AdditionalText> additionalTexts)
    {
        RunNumber = runNumber;
        Verifier = verifier;
        AdditionalTexts = additionalTexts;
        TestState = testState;
        PreGeneratorCompilation = preGeneratorCompilation;
        Driver = driver;
    }

    internal IncrementalRun RunGenerator()
    {
        var newDriver = Driver.RunGeneratorsAndUpdateCompilation(PreGeneratorCompilation.Compilation, out var outputCompilation, out var diagnostics);

        var runResult = newDriver.GetRunResult();
        var generatorDiagnostics = runResult.Results.SelectMany(r => r.Diagnostics);
        var compilationDiagnostics = outputCompilation.GetDiagnostics();

        // Create a new verifier with a new context for each run
        var verifier = Verifier.PushContext($"Run {RunNumber}");
        verifier.VerifyGeneratedSources(TestState, runResult);
        verifier.VerifyZeroDiagnostics(TestState, generatorDiagnostics, "generator", TestBehaviour.SkipGeneratorDiagnostic);
        verifier.VerifyZeroDiagnostics(TestState, compilationDiagnostics, "compilation", TestBehaviour.SkipCompilationDiagnostic);

        // Pass the original verifier
        return new IncrementalRun(RunNumber, Verifier, TestState, PreGeneratorCompilation, newDriver, AdditionalTexts);
    }

    internal IncrementalRun ApplyIncrementalChange(TestState newState)
    {
        var oldState = TestState;
        var newCompilation = PreGeneratorCompilation;

        var oldSources = oldState.Sources.Select(a => a.HintPath).ToHashSet();
        var newSources = newState.Sources.Select(a => a.HintPath).ToHashSet();

        // Remove sources from previous state that are not in new state
        foreach (var source in oldSources.Where(old => !newSources.Contains(old)))
        {
            newCompilation = newCompilation.RemoveSource(source);
        }

        // Add or update sources for the new state
        foreach (var (hintPath, content) in newState.Sources)
        {
            newCompilation = newCompilation.SetSource(hintPath, content);
        }

        var newAdditionalTexts = CreateAdditionalTexts(newState);
        var newDriver = Driver.ReplaceAdditionalTexts(newAdditionalTexts.CastArray<Microsoft.CodeAnalysis.AdditionalText>());

        // Check if analyzer config options have changed
        if (!oldState.AnalyzerConfigOptions.SequenceEqual(newState.AnalyzerConfigOptions))
        {
            newDriver = newDriver.WithUpdatedAnalyzerConfigOptions(new AnalyzerConfigOptionsProvider(newState.AnalyzerConfigOptions, newState.AnalyzerConfigOptionsFactory));
        }

        // Increment run number and pass updated run state
        return new IncrementalRun(RunNumber + 1, Verifier, newState, newCompilation, newDriver, newAdditionalTexts);
    }

    private static IncrementalCompilation CreateCompilation(TestState testState, ImmutableArray<MetadataReference> references)
    {
        var incrementalCompilation = new IncrementalCompilation(references);

        foreach (var sourceFile in testState.Sources)
        {
            incrementalCompilation = incrementalCompilation.SetSource(sourceFile.HintPath, sourceFile.Content);
        }

        return incrementalCompilation;
    }

    private static GeneratorDriver CreateDriver(IIncrementalGenerator sourceGen, TestState testState, ImmutableArray<AdditionalText> additionalTexts)
    {
        var driver = CSharpGeneratorDriver.Create(sourceGen)
            .WithUpdatedParseOptions(CSharpParseOptions.Default.WithLanguageVersion(testState.LanguageVersion))
            .WithUpdatedAnalyzerConfigOptions(new AnalyzerConfigOptionsProvider(testState.AnalyzerConfigOptions, testState.AnalyzerConfigOptionsFactory))
            .AddAdditionalTexts(additionalTexts.CastArray<Microsoft.CodeAnalysis.AdditionalText>());

        return driver;
    }

    private static ImmutableArray<AdditionalText> CreateAdditionalTexts(TestState testState)
    {
        return testState.AdditionalText
            .Select(text => new AdditionalText(text.HintPath, text.Content)).ToImmutableArray();
    }
}