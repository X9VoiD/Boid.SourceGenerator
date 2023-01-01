using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Boid.SourceGenerator.Testing;

public abstract class IncrementalRunObserver
{
    private static Dictionary<IIncrementalGenerator, ImmutableArray<IncrementalRunObserver>>? _observers;

    internal static void RegisterGenerator(IIncrementalGenerator generator, ImmutableArray<IncrementalRunObserver> observers)
    {
        _observers ??= new();
        ref var registeredObservers = ref CollectionsMarshal.GetValueRefOrAddDefault(_observers, generator, out _);
        registeredObservers = observers;
    }

    internal static void UnregisterGenerator(IIncrementalGenerator generator)
    {
        _observers?.Remove(generator);
    }

    public int RunNumber { get; protected set; }

    public event Action<IncrementalRunObserver, IVerifier>? Verify;

    public IncrementalRunObserver AddVerifier(Action<IncrementalRunObserver, IVerifier> verify)
    {
        Verify += verify;
        return this;
    }

    protected virtual void OnRunStart(int runNumber)
    {
        RunNumber = runNumber;
    }

    protected virtual void OnRunEnd()
    {
    }

    protected virtual void OnCustomEvent(string eventName, object? data)
    {
    }

    internal void OnRunStartInternal(int runNumber)
    {
        OnRunStart(runNumber);
    }

    internal void OnRunEndInternal()
    {
        OnRunEnd();
    }

    internal void OnCustomEventInternal(string eventName, object? data)
    {
        OnCustomEvent(eventName, data);
    }

    internal void RunVerifiers(IVerifier verifier)
    {
        Verify?.Invoke(this, verifier);
    }

    [Conditional("DEBUG")]
    public static void InvokeEvent(IIncrementalGenerator generator, string eventName, object? data)
    {
        if (_observers is null)
            return;

        if (!_observers.TryGetValue(generator, out var observers))
            return;

        observers.ForEach(o => o.OnCustomEvent(eventName, data));
    }
}