# Boid.SourceGenerator.Testing

Test incremental generators with ease!

## What it does?

This provides a simple way to test incremental generators, including the ability
to test incremental changes.

### Installation

Add the package `Boid.SourceGenerator.Testing` to your test project.

```xml
<ItemGroup>
  <PackageReference Include="Boid.SourceGenerator.Testing" Version="0.1.0" />
</ItemGroup>
```

> Refer to [NuGet](https://www.nuget.org/packages/Boid.SourceGenerator.Testing)
> for the latest version.

And then add either of the following depending on the testing framework you use:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.CodeAnalysis.Testing.Verifiers.MSTest" Version="0.1.0" />
  <PackageReference Include="Microsoft.CodeAnalysis.Testing.Verifiers.XUnit" Version="0.1.0" />
  <PackageReference Include="Microsoft.CodeAnalysis.Testing.Verifiers.NUnit" Version="0.1.0" />
</ItemGroup>
```

### Setup

Decide on a folder to store the test files to assert against. This is `<ProjectDirectory>/TestResources`
by default, but can be changed by setting the `TestResource.RelativeDirectory` property.

```cs
// Sets the directory to search for test resources to <ProjectDirectory>/TestFiles
TestResource.RelativeDirectory = "TestFiles";
```

The structure of the test resource directory should be as follows:

```
TestResources
├── <TestName>
│   ├── Sources
│   │   ├── <Source1>.cs
│   │   ├── <Source2>.cs
│   │   └── ...
│   ├── Generated
│   │   ├── <Generated1>.cs
│   │   ├── <Generated2>.cs
│   │   └── ...
│   ├── AdditionalText
│   │   ├── <AdditionalText1>
│   │   ├── <AdditionalText2>
│   │   └── ...
│   └── AnalyzerConfigOptions
│       ├── global.editorconfig
│       ├── <AnalyzerConfigOptions1>.editorconfig
│       ├── <AnalyzerConfigOptions2>.editorconfig
│       └── ...
└── ...
```

where:

- `<TestName>` is the name of the test.
- `<Source*>.cs` is the source files that will be fed to the generator.
- `<Generated*>.cs` is the expected generated files.
- `<AdditionalText*>` is the additional text files returned by an `AdditionalTextsProvider`.
- `global.editorconfig` will dictate the output of `AnalyzerConfigOptionsProvider.GlobalOptions`.
- `<AnalyzerConfigOptions*>` is the analyzer config options to be associated to
  an additional text of the same filename.

> More about the analyzer config options in [MSBuild properties and metadata](#msbuild-properties-and-metadata)
> section.

### Usage

The following examples will assume that the testing framework is `Xunit`.

#### Single state testing

This is the simplest way to test an incremental generator with multiple test
cases to go through.

```cs
public class GeneratorTest
{
    [Theory]
    [InlineData("Test1")]
    [InlineData("Test2")]
    public async Task SingleStateTest(string testName)
    {
        await new IncrementalGeneratorVerifier<MyIncrementalGenerator, XUnitVerifier>()
        {
            TestState = new TestState(testName)
        }.RunAsync();
    }
}
```

#### Multi state testing

This allows one to test how the incremental generator behaves when doing incremental
changes to the source files, additional texts, or analyzer config options.

```cs
public class GeneratorTest
{
    [Fact]
    public async Task Generator_Behaves_Well_On_Code_Edits()
    {
        await new IncrementalGeneratorVerifier<MyIncrementalGenerator, XUnitVerifier>()
            .RunIncrementalAsync(
                new TestState("Test1"),
                new TestState("Test2"),
                new TestState("Test3")
            );
    }
}
```

### MSBuild properties and metadata

To test analyzer config options, create an `editorconfig` file with the same name
as the additional text file. For example, if the additional text file is named
`MyAdditionalText.txt`, then the analyzer config options file should be named
`MyAdditionalText.txt.editorconfig`.

The content of the `editorconfig` file should be in the following format:

```
<AnalyzerConfigOption1> = <Value1>
<AnalyzerConfigOption2> = <Value2>
...
```

For example:

```
build_property.MyProperty = MyValue
build_property.MyOtherProperty = MyOtherValue
```

#### Compiler-visible properties

To add compiler-visible properties, prefix the property name with `build_property`.

Compiler-visible properties refers to the MSBuild properties that are explicitly
made visible to the generator via the `CompilerVisibleProperty` and `CompilerVisibleItemMetadata`
item groups.

For example, the `editorconfig` above is equivalent to the following MSBuild properties:

```xml
<PropertyGroup>
  <MyProperty>MyValue</MyProperty>
  <MyOtherProperty>MyOtherValue</MyOtherProperty>
</PropertyGroup>
```

and the following `CompilerVisibleProperty` item group:

```xml
<ItemGroup>
  <CompilerVisibleProperty Include="MyProperty" />
  <CompilerVisibleProperty Include="MyOtherProperty" />
</ItemGroup>
```

#### Additional Text metadata

To associate metadata to an additional text, use the prefix `build_metadata.AdditionalFiles`.

For example, the following MSBuild items:

```xml
<ItemGroup>
  <AdditionalFiles Include="MyAdditionalText.txt" MyMetadata="MyValue" />
</ItemGroup>
```

is equivalent to `MyAdditionalText.txt.editorconfig`:

```
build_metadata.AdditionalFiles.MyMetadata = MyValue
```