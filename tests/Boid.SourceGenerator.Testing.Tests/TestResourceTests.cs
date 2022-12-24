namespace Boid.SourceGenerator.Testing.Tests;

public class TestResourceTests
{
    [Fact]
    public void ProjectDir_Returns_ProjectDir()
    {
        Assert.Equal("Boid.SourceGenerator.Testing.Tests", Path.GetFileName(TestResource.ProjectDir));
    }

    [Fact]
    public void TestResourcesRelativeDir_Returns_Default()
    {
        Assert.Equal("TestResources", TestResource.TestResourcesRelativeDir);
    }

    [Fact]
    public void TestResourcesDir_Returns_TestResourcesRelativeDir_By_Default()
    {
        Assert.Equal("TestResources", Path.GetRelativePath(TestResource.ProjectDir, TestResource.TestResourcesDir));
    }

    [Fact]
    public void TestResourcesDir_Returns_ProjectDirAndTestResourcesRelativeDir()
    {
        TestResource.ProjectDir = "/example/project";
        TestResource.TestResourcesRelativeDir = "resources";

        Assert.Equal("/example/project/resources", TestResource.TestResourcesDir);
    }

    [Fact]
    public void GetTestResource_When_NonExistent_Throws_Exception()
    {
        TestResource.ProjectDir = "/example/project";

        var exception = Assert.Throws<DirectoryNotFoundException>(() => TestResource.GetTestResource("non-existent"));

        Assert.Equal("Test resource directory not found: /example/project/TestResources/non-existent", exception.Message);
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
        var testResource = TestResource.GetTestResource("A");

        Assert.Equal("A", testResource.Name);
        Assert.Single(testResource.Sources);
        Assert.Equal("A.cs", testResource.Sources[0].HintPath);
        Assert.Equal("public class A { }", testResource.Sources[0].Content);
    }

    [Fact]
    public void GetTestResources_Returns_TestResources()
    {
        var testResources = TestResource.GetTestResources();

        Assert.NotEmpty(testResources);
    }
}