using Microsoft.CodeAnalysis;

namespace SampleGenerators;

[Generator]
public class ConditionalStaticCodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var shouldGenerate = context.AnalyzerConfigOptionsProvider
            .Select((p, ct) => p.GlobalOptions.TryGetValue("build_property.ConditionalStaticCodeGenerator_GenerateClass", out var value) && value == "true");

        context.RegisterSourceOutput(shouldGenerate, (ctx, shouldGenerate) =>
        {
            if (shouldGenerate)
            {
                var source = "public class ConditionalStaticCodeGeneratorGeneratedClass { }";
                ctx.AddSource("ConditionalStaticCodeGenerator.g.cs", source);
            }
        });
    }
}