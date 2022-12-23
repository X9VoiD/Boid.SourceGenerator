namespace Boid.SourceGenerator.Testing;

public interface IIncrementalRunObserver
{
    void OnRunStart(int runNumber);
    void OnRunEnd(int runNumber);
    void OnCustomEvent(int runNumber, string eventName, object? data);
}