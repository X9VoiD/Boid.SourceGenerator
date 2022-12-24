using System.Collections.Immutable;

namespace Boid.SourceGenerator.Testing;

public class TestResource
{
    public static string ProjectDir { get; set; } = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    public static string TestResourcesRelativeDir { get; set; } = "TestResources";
    public static string TestResourcesDir => Path.Combine(ProjectDir, TestResourcesRelativeDir);

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
        var testResourcesDir = TestResourcesDir;
        if (!Directory.Exists(testResourcesDir))
            throw new DirectoryNotFoundException($"Test resources directory not found: {testResourcesDir}");

        return Directory.EnumerateDirectories(testResourcesDir)
            .Select(GetTestResource)
            .ToImmutableArray();
    }

    public static TestResource GetTestResource(string name)
    {
        var resourceDir = Path.Combine(TestResourcesDir, name);
        if (!Directory.Exists(resourceDir))
            throw new DirectoryNotFoundException($"Test resource directory not found: {resourceDir}");

        var sources = GetFiles(Path.Combine(resourceDir, "Sources"));
        var generated = GetFiles(Path.Combine(resourceDir, "Generated"));
        var additionalFiles = GetFiles(Path.Combine(resourceDir, "AdditionalText"));
        var analyzerConfigOptions = GetAnalyzerFiles(Path.Combine(resourceDir, "AnalyzerConfigOptions"));

        return new TestResource(name, sources, generated, additionalFiles, analyzerConfigOptions);
    }

    private static ImmutableArray<TestFile> GetFiles(string path)
    {
        return Directory.EnumerateFiles(path, "*.cs")
            .Select(f => new TestFile(Path.GetRelativePath(path, f), File.ReadAllText(f)))
            .ToImmutableArray();
    }

    private static ImmutableArray<AnalyzerConfigOptionsFile> GetAnalyzerFiles(string path)
    {
        return Directory.EnumerateFiles(path, "*.cs")
            .Select(f => new AnalyzerConfigOptionsFile(Path.GetRelativePath(path, f), File.ReadAllText(f)))
            .ToImmutableArray();
    }
}
