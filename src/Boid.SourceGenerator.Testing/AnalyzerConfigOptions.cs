using System.Diagnostics.CodeAnalysis;

namespace Boid.SourceGenerator.Testing;

internal sealed class AnalyzerConfigOptions : global::Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
{
    private readonly Dictionary<string, string> _options = new();

    public void Add(string key, string value)
    {
        _options.Add(key, value);
    }

    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        return _options.TryGetValue(key, out value);
    }
}