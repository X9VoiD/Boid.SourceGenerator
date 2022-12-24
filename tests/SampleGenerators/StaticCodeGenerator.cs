using Microsoft.CodeAnalysis;

namespace SampleGenerators
{
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
}
