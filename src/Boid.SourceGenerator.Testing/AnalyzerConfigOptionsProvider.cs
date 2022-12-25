using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Boid.SourceGenerator.Testing;

internal sealed partial class AnalyzerConfigOptionsProvider : global::Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
{
    private readonly Dictionary<string, AnalyzerConfigOptions> _options = new();
    private readonly AnalyzerConfigOptions _globalOptions = new();

    public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

    /// <summary>
    /// Sets a function delegate that returns additional options for a given path.
    /// This is called when no options are configured for the given path.
    /// </summary>
    public Func<string, string?>? AdditionalConfigOptionsFactory { get; }

    public AnalyzerConfigOptionsProvider(Func<string, string?>? additionalConfigOptionsFactory = null)
    {
        AdditionalConfigOptionsFactory = additionalConfigOptionsFactory;
    }

    public AnalyzerConfigOptionsProvider(IEnumerable<AnalyzerConfigOptionsFile> analyzerConfigOptions, Func<string, string?>? additionalConfigOptionsFactory = null)
        : this(additionalConfigOptionsFactory)
    {
        foreach (var (file, content) in analyzerConfigOptions)
        {
            From(file, content);
        }
    }

    public void From(string filePath, string content)
    {
        filePath = EditorConfigExt().Match(filePath).Groups["fileName"].Captures[0].Value;

        var matches = (IEnumerable<Match>)ValuePair().Matches(content)!;
        if (filePath == "global")
        {
            foreach (var match in matches)
            {
                AddGlobal(match.Groups["key"].Captures[0].Value, match.Groups["value"].Captures[0].Value);
            }
        }
        else
        {
            foreach (var match in matches)
            {
                Add(filePath, match.Groups["key"].Captures[0].Value, match.Groups["value"].Captures[0].Value);
            }
        }
    }

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
    {
        return GetOptions(Path.GetFileName(tree.FilePath));
    }

    public override AnalyzerConfigOptions GetOptions(Microsoft.CodeAnalysis.AdditionalText textFile)
    {
        return GetOptions(Path.GetFileName(textFile.Path));
    }

    private AnalyzerConfigOptions GetOptions(string path)
    {
        if (!_options.TryGetValue(path, out var options))
        {
            if (AdditionalConfigOptionsFactory != null)
            {
                var content = AdditionalConfigOptionsFactory(path);
                if (content != null)
                {
                    From(path, content);
                    _options.TryGetValue(path, out options);
                }
            }

            options ??= new AnalyzerConfigOptions();
        }

        return options;
    }

    private void Add(string filePath, string key, string value)
    {
        var path = Path.GetFileName(filePath);
        if (!_options.TryGetValue(path, out var options))
        {
            options = new AnalyzerConfigOptions();
            _options.Add(path, options);
        }

        options.Add(key, value);
    }

    private void AddGlobal(string key, string value)
    {
        _globalOptions.Add(key, value);
    }

    [GeneratedRegex(@"^(?<fileName>.*)\.editorconfig$", RegexOptions.NonBacktracking)]
    private static partial Regex EditorConfigExt();

    [GeneratedRegex(@"^(?<key>(?:\w|_|-|\.)+) = (?<value>(?:\w|_|-|\.)+)$", RegexOptions.Multiline | RegexOptions.NonBacktracking)]
    private static partial Regex ValuePair();
}
