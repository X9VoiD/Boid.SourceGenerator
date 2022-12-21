namespace Boid.SourceGenerator.Testing;

public class AnalyzerConfigOptionsFile
{
    private readonly string _hintPath;
    private readonly string _content;

    public AnalyzerConfigOptionsFile(string hintPath, string content)
    {
        _hintPath = hintPath;
        _content = content;
    }

    internal void Deconstruct(out string hintPath, out string content)
    {
        hintPath = _hintPath;
        content = _content;
    }

    public static implicit operator (string, string)(AnalyzerConfigOptionsFile file) => (file._hintPath, file._content);
}