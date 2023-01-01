using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Boid.SourceGenerator.Testing;

public class Counter : IncrementalRunObserver
{
    public string Name { get; }
    public int Count { get; private set; }

    public Counter(string name)
    {
        Name = name;
    }

    public static void Increment(IIncrementalGenerator generator, string counterName)
    {
        InvokeEvent(generator, nameof(Counter), counterName);
    }

    public static Action<IncrementalRunObserver, IVerifier> PerfectCaching()
    {
        var firstHits = 0;
        return (observer, verifier) =>
        {
            var counter = (Counter)observer;
            if (counter.RunNumber == 1)
            {
                firstHits = counter.Count;
            }
            else
            {
                verifier.Equal(firstHits, counter.Count);
            }
        };
    }

    public static Action<IncrementalRunObserver, IVerifier> CounterHits(IEnumerable<int> expectedHits)
    {
        ArgumentNullException.ThrowIfNull(expectedHits);

        var expectedHitsEnumerator = expectedHits.GetEnumerator();
        return (observer, verifier) =>
        {
            var counter = (Counter)observer;
            if (expectedHitsEnumerator.MoveNext())
            {
                verifier.Equal(expectedHitsEnumerator.Current, counter.Count);
            }
            else
            {
                verifier.Fail("Too many runs");
            }
        };
    }

    protected override void OnCustomEvent(string eventName, object? data)
    {
        if (eventName == nameof(Counter) && data is string counterName && counterName == Name)
        {
            Count++;
        }
    }

    public override string ToString()
    {
        return $"{Name}: {Count}";
    }
}