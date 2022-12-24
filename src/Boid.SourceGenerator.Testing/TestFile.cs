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

    public static (string, string) ToValueTuple(TestFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        return (file.HintPath, file.Content);
    }
}