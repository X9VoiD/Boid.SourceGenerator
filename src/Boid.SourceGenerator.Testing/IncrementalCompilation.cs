using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Boid.SourceGenerator.Testing;

internal sealed class IncrementalCompilation
{
    private readonly ImmutableDictionary<string, SyntaxTree> _sources = ImmutableDictionary<string, SyntaxTree>.Empty;

    public Compilation Compilation { get; }

    public IncrementalCompilation(ImmutableArray<MetadataReference> references)
    {
        Compilation = CSharpCompilation.Create("test",
            references: references.Append(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private IncrementalCompilation(ImmutableDictionary<string, SyntaxTree> sources, Compilation compilation)
    {
        _sources = sources;
        Compilation = compilation;
    }

    public IncrementalCompilation SetSource(string filename, string content)
    {
        var newTree = CSharpSyntaxTree.ParseText(content, path: filename);

        if (_sources.TryGetValue(filename, out var oldTree))
        {
            if (newTree.ToString() == oldTree.ToString())
            {
                return this;
            }

            var newSources = _sources.SetItem(filename, newTree);
            var newCompilation = Compilation.ReplaceSyntaxTree(oldTree, newTree);

            return new IncrementalCompilation(newSources, newCompilation);
        }
        else
        {
            var newSources = _sources.Add(filename, newTree);
            var newCompilation = Compilation.AddSyntaxTrees(newTree);

            return new IncrementalCompilation(newSources, newCompilation);
        }
    }

    public IncrementalCompilation RemoveSource(string filename)
    {
        var oldTree = _sources[filename];

        var newSources = _sources.Remove(filename);
        var newCompilation = Compilation.RemoveSyntaxTrees(oldTree);

        return new IncrementalCompilation(newSources, newCompilation);
    }
}