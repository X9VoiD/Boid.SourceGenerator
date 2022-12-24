namespace Boid.SourceGenerator.Testing;

public class AnalyzerConfigOptionsFile : IEquatable<AnalyzerConfigOptionsFile>
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

    public bool Equals(AnalyzerConfigOptionsFile? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _hintPath == other._hintPath && _content == other._content;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is AnalyzerConfigOptionsFile other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_hintPath, _content);
    }

    public static bool operator ==(AnalyzerConfigOptionsFile? left, AnalyzerConfigOptionsFile? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AnalyzerConfigOptionsFile? left, AnalyzerConfigOptionsFile? right)
    {
        return !Equals(left, right);
    }

    public static (string, string) ToValueTuple(AnalyzerConfigOptionsFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        return (file._hintPath, file._content);
    }
}