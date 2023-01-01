using Microsoft.CodeAnalysis;

namespace Boid.SourceGenerator.Testing.Tests.Generators;

// Unconditionally adds 'generated.cs' to the compilation with just
// 'public class GeneratedClass { }' in it

public class StaticCodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            Counter.Increment(this, nameof(StaticCodeGenerator));
            ctx.AddSource("generated.cs", "public class GeneratedClass { }");
        });
    }
}
