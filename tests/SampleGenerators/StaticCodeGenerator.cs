using Microsoft.CodeAnalysis;

namespace SampleGenerators;

// Unconditionally adds 'generated.cs' to the compilation with just
// 'public class GeneratedClass { }' in it

[Generator]
public class StaticCodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("generated.cs", "public class GeneratedClass { }");
        });
    }
}
