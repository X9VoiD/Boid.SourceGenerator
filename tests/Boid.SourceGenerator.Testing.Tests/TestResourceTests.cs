namespace Boid.SourceGenerator.Testing.Tests;

public class TestResourceTests
{
    [Fact]
    public void ProjectDir_Returns_ProjectDir()
    {
        Assert.Equal("Boid.SourceGenerator.Testing.Tests", Path.GetFileName(TestResource.ProjectDirectory));
    }

    [Fact]
    public void TestResourcesRelativeDir_Returns_Default()
    {
        Assert.Equal("TestResources", TestResource.RelativeDirectory);
    }

    [Fact]
    public void TestResourcesDir_Returns_TestResourcesRelativeDir_By_Default()
    {
        Assert.Equal("TestResources", Path.GetRelativePath(TestResource.ProjectDirectory, TestResource.ResourcesDirectory));
    }

    [Fact]
    public void TestResourcesDir_Returns_ProjectDirAndTestResourcesRelativeDir()
    {
        Isolate(() =>
        {
            TestResource.ProjectDirectory = "/example/project";
            TestResource.RelativeDirectory = "resources";

            Assert.Equal("/example/project/resources", TestResource.ResourcesDirectory);
        });
    }

    [Fact]
    public void GetTestResource_When_NonExistent_Throws_Exception()
    {
        Isolate(() =>
        {
            TestResource.ProjectDirectory = "/example/project";

            var exception = Assert.Throws<DirectoryNotFoundException>(() => TestResource.GetTestResource("non-existent"));

            Assert.Equal("Test resource directory not found: /example/project/TestResources/non-existent", exception.Message);
        });
    }

    [Fact]
    public void GetTestResource_When_SubdirectoryEmpty_Returns_Empty()
    {
        var testResource = TestResource.GetTestResource("Empty");

        Assert.Equal("Empty", testResource.Name);
        Assert.Empty(testResource.Sources);
        Assert.Empty(testResource.Generated);
        Assert.Empty(testResource.AdditionalText);
        Assert.Empty(testResource.AnalyzerConfigOptions);
        Assert.True(testResource.IsEmpty);
    }

    [Fact]
    public void GetTestResource_Returns_TestResource()
    {
        var testResource = TestResource.GetTestResource("EmptyClass");

        Assert.Equal("EmptyClass", testResource.Name);
        Assert.Single(testResource.Sources);
        Assert.Equal("EmptyClass.cs", testResource.Sources[0].HintPath);
        Assert.Equal("public class EmptyClass { }", testResource.Sources[0].Content);
    }

    [Fact]
    public void GetTestResources_Returns_TestResources()
    {
        var testResources = TestResource.GetTestResources();

        Assert.NotEmpty(testResources);
    }

    private static void Isolate(Action action)
    {
        var projectDir = TestResource.ProjectDirectory;
        var testResourcesRelativeDir = TestResource.RelativeDirectory;

        try
        {
            action();
        }
        finally
        {
            TestResource.ProjectDirectory = projectDir;
            TestResource.RelativeDirectory = testResourcesRelativeDir;
        }
    }
}