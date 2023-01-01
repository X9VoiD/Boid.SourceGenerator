namespace Boid.SourceGenerator.Testing;

[Flags]
public enum TestBehaviour
{
    None,
    SkipGeneratedSource = 1,
    SkipCompilationDiagnostic = 2,
    SkipGeneratorDiagnostic = 4,
    SkipObserverVerification = 8,
}