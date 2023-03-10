using System.Collections.Immutable;

namespace Boid.SourceGenerator.Testing;

public class TestResource
{
    public static string ProjectDirectory { get; set; } = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    public static string RelativeDirectory { get; set; } = "TestResources";
    public static string ResourcesDirectory => Path.Combine(ProjectDirectory, RelativeDirectory);

    public string Name { get; }

    public ImmutableArray<TestFile> Sources { get; }
    public ImmutableArray<TestFile> Generated { get; }
    public ImmutableArray<TestFile> AdditionalText { get; }
    public ImmutableArray<AnalyzerConfigOptionsFile> AnalyzerConfigOptions { get; }

    public bool IsEmpty => Sources.IsEmpty
        && Generated.IsEmpty
        && AdditionalText.IsEmpty
        && AnalyzerConfigOptions.IsEmpty;

    public TestResource(
        string name,
        ImmutableArray<TestFile> sources,
        ImmutableArray<TestFile> generated,
        ImmutableArray<TestFile> additionalText,
        ImmutableArray<AnalyzerConfigOptionsFile> analyzerConfigOptions)
    {
        Name = name;
        Sources = sources;
        Generated = generated;
        AdditionalText = additionalText;
        AnalyzerConfigOptions = analyzerConfigOptions;
    }

    public static ImmutableArray<TestResource> GetTestResources()
    {
        var testResourcesDir = ResourcesDirectory;
        if (!Directory.Exists(testResourcesDir))
            throw new DirectoryNotFoundException($"Test resources directory not found: {testResourcesDir}");

        return Directory.EnumerateDirectories(testResourcesDir)
            .Select(GetTestResource)
            .ToImmutableArray();
    }

    public static TestResource GetTestResource(string name)
    {
        var resourceDir = Path.Combine(ResourcesDirectory, name);
        if (!Directory.Exists(resourceDir))
            throw new DirectoryNotFoundException($"Test resource directory not found: {resourceDir}");

        var sources = GetFiles(Path.Combine(resourceDir, "Sources"), ".cs");
        var generated = GetFiles(Path.Combine(resourceDir, "Generated"), ".cs");
        var additionalTexts = GetFiles(Path.Combine(resourceDir, "AdditionalText"));
        var analyzerConfigOptions = GetAnalyzerFiles(Path.Combine(resourceDir, "AnalyzerConfigOptions"));

        return new TestResource(name, sources, generated, additionalTexts, analyzerConfigOptions);
    }

    private static ImmutableArray<TestFile> GetFiles(string path, string? extension = null)
    {
        return GetFiles(path, extension, f => new TestFile(Path.GetRelativePath(path, f), File.ReadAllText(f)));
    }

    private static ImmutableArray<AnalyzerConfigOptionsFile> GetAnalyzerFiles(string path)
    {
        return GetFiles(path, ".editorconfig", f => new AnalyzerConfigOptionsFile(Path.GetRelativePath(path, f), File.ReadAllText(f)));
    }

    private static ImmutableArray<T> GetFiles<T>(string path, string? extension, Func<string, T> selector)
    {
        if (!Directory.Exists(path))
            return ImmutableArray<T>.Empty;

        var files = extension == null
            ? Directory.EnumerateFiles(path)
            : Directory.EnumerateFiles(path, $"*{extension}");

        return files.Select(selector)
            .ToImmutableArray();
    }
}
