using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Boid.SourceGenerator.Testing;

internal sealed class AdditionalText : global::Microsoft.CodeAnalysis.AdditionalText
{
    private readonly SourceText _text;

    public override string Path { get; }

    public AdditionalText(string file, string content)
    {
        Path = file;
        _text = SourceText.From(content, encoding: Encoding.UTF8);
    }

    public override SourceText? GetText(CancellationToken cancellationToken = default)
    {
        return _text;
    }
}