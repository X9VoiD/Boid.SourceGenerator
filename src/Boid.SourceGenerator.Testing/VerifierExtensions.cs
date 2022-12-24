using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Boid.SourceGenerator.Testing;

internal static class VerifierExtensions
{
    public static void VerifyGeneratedSources(this IVerifier verifier, TestState testState, GeneratorDriverRunResult runResult)
    {
        if (testState.TestBehaviour.HasFlag(TestBehaviour.SkipGeneratedSource))
            return;

        var expected = testState.Generated;
        var generatedSources = runResult.Results
                                        .SelectMany(r => r.GeneratedSources)
                                        .ToDictionary(s => s.HintName);

        if (generatedSources.Count != expected.Length)
            verifier.Equal(expected.Length, generatedSources.Count, $"Expected {expected.Length} generated sources, but found {generatedSources.Count}");

        foreach (var (file, content) in expected)
        {
            if (!generatedSources.TryGetValue(file, out var source))
                verifier.Fail($"Expected generated source '{file}', but it was not found");

            var actual = source.SourceText.ToString();

            verifier.EqualOrDiff(content, actual, $"Generated source '{file}' did not match expected content");
        }
    }

    public static void VerifyZeroDiagnostics(this IVerifier verifier, TestState testState, IEnumerable<Diagnostic> diagnostics, string context, TestBehaviour flag)
    {
        if (testState.TestBehaviour.HasFlag(flag))
            return;

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();

        if (errors.Length > 0)
        {
            var sb = new StringBuilder();

            sb.AppendLine(null, $"Expected no diagnostics, but found {errors.Length} in {context}:");

            foreach (var d in errors)
                sb.AppendLine(null, $"    {d}");

            verifier.Fail(sb.ToString());
        }
    }
}