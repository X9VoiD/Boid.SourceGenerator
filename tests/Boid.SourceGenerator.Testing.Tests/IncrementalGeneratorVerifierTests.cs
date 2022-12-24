using Microsoft.CodeAnalysis.Testing.Verifiers;
using SampleGenerators;
using Xunit.Sdk;

namespace Boid.SourceGenerator.Testing.Tests;

public class IncrementalGeneratorVerifierTests
{
    [Fact]
    public async Task RunAsync_Fails_Assertion_When_Invalid_NumberOf_GeneratedSources()
    {
        var verifier = new IncrementalGeneratorVerifier<StaticCodeGenerator, XUnitVerifier>()
        {
            TestState = new TestState("EmptyClass")
        };

        var exception = await Assert.ThrowsAsync<EqualWithMessageException>(() => verifier.RunAsync());

        Assert.Equal("0", exception.Expected);
        Assert.Equal("1", exception.Actual);
        Assert.Equal("Expected 0 generated sources, but found 1", exception.UserMessage);
    }

    [Fact]
    public async Task RunAsync_Fails_Assertion_When_Missing_GeneratedSource()
    {
        // Test resource 'GeneratedClassWrongFileName' expects a generated source with the name 'benerated.cs',
        // but the generator generates one with the name 'generated.cs'.
        var verifier = new IncrementalGeneratorVerifier<StaticCodeGenerator, XUnitVerifier>()
        {
            TestState = new TestState("GeneratedClassWrongFileName")
        };

        var exception = await Assert.ThrowsAnyAsync<TrueException>(() => verifier.RunAsync());

        Assert.Equal("Expected generated source 'benerated.cs', but it was not found", exception.UserMessage);
    }
}