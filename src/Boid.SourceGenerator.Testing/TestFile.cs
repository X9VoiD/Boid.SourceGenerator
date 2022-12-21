namespace Boid.SourceGenerator.Testing;

public class TestFile
{
    public string HintPath { get; }
    public string Content { get; }

    public TestFile(string hintPath, string content)
    {
        HintPath = hintPath;
        Content = content;
    }

    public void Deconstruct(out string hintPath, out string content)
    {
        hintPath = HintPath;
        content = Content;
    }

    public static implicit operator (string, string)(TestFile file) => (file.HintPath, file.Content);
}