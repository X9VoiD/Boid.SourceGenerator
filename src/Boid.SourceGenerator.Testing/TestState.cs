using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

namespace Boid.SourceGenerator.Testing;

public record TestState
{
    public LanguageVersion LanguageVersion { get; init; } = LanguageVersion.Default;
    public ImmutableArray<TestFile> Sources { get; init; } = ImmutableArray<TestFile>.Empty;
    public ImmutableArray<TestFile> Generated { get; init; } = ImmutableArray<TestFile>.Empty;
    public ImmutableArray<TestFile> AdditionalText { get; init; } = ImmutableArray<TestFile>.Empty;
    public ImmutableArray<AnalyzerConfigOptionsFile> AnalyzerConfigOptions { get; init; } = ImmutableArray<AnalyzerConfigOptionsFile>.Empty;
    public TestBehaviour TestBehaviour { get; init; }

    /// <summary>
    /// Sets a function delegate that returns additional options for the given path.
    /// This is called when no options are configured for the given path.
    /// </summary>
    public Func<string, string?>? AnalyzerConfigOptionsFactory { get; init; }

    public TestState WithLanguageVersion(LanguageVersion languageVersion)
        => this with { LanguageVersion = languageVersion };

    public TestState WithSources(ImmutableArray<TestFile> sources)
        => this with { Sources = sources };

    public TestState WithSources(params TestFile[] sources)
        => this with { Sources = sources.ToImmutableArray() };

    public TestState WithGenerated(ImmutableArray<TestFile> generated)
        => this with { Generated = generated };

    public TestState WithGenerated(params TestFile[] generated)
        => this with { Generated = generated.ToImmutableArray() };

    public TestState WithAdditionalText(ImmutableArray<TestFile> additionalText)
        => this with { AdditionalText = additionalText };

    public TestState WithAdditionalText(params TestFile[] additionalText)
        => this with { AdditionalText = additionalText.ToImmutableArray() };

    public TestState WithAnalyzerConfigOptions(ImmutableArray<AnalyzerConfigOptionsFile> analyzerConfigOptions)
        => this with { AnalyzerConfigOptions = analyzerConfigOptions };

    public TestState WithAnalyzerConfigOptions(params AnalyzerConfigOptionsFile[] analyzerConfigOptions)
        => this with { AnalyzerConfigOptions = analyzerConfigOptions.ToImmutableArray() };

    public TestState WithTestBehaviour(TestBehaviour testBehaviour)
        => this with { TestBehaviour = testBehaviour };

    public TestState AddSources(ImmutableArray<TestFile> sources)
        => this with { Sources = Sources.AddRange(sources) };

    public TestState AddSources(params TestFile[] sources)
        => this with { Sources = Sources.AddRange(sources) };

    public TestState AddGenerated(ImmutableArray<TestFile> generated)
        => this with { Generated = Generated.AddRange(generated) };

    public TestState AddGenerated(params TestFile[] generated)
        => this with { Generated = Generated.AddRange(generated) };

    public TestState AddAdditionalText(ImmutableArray<TestFile> additionalText)
        => this with { AdditionalText = AdditionalText.AddRange(additionalText) };

    public TestState AddAdditionalText(params TestFile[] additionalText)
        => this with { AdditionalText = AdditionalText.AddRange(additionalText) };

    public TestState AddAnalyzerConfigOptions(ImmutableArray<AnalyzerConfigOptionsFile> analyzerConfigOptions)
        => this with { AnalyzerConfigOptions = AnalyzerConfigOptions.AddRange(analyzerConfigOptions) };

    public TestState AddAnalyzerConfigOptions(params AnalyzerConfigOptionsFile[] analyzerConfigOptions)
        => this with { AnalyzerConfigOptions = AnalyzerConfigOptions.AddRange(analyzerConfigOptions) };

    public static TestState FromResource(TestResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        if (resource.IsEmpty)
            throw new ArgumentException("Cannot create TestState from empty TestResource.", nameof(resource));

        return new TestState
        {
            Sources = resource.Sources,
            Generated = resource.Generated,
            AdditionalText = resource.AdditionalText,
            AnalyzerConfigOptions = resource.AnalyzerConfigOptions,
        };
    }
}