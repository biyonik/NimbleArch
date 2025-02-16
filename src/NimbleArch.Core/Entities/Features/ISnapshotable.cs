namespace NimbleArch.Core.Entities.Features;

public interface ISnapshotable<TSnapshot> where TSnapshot : class
{
    TSnapshot CreateSnapshot();
    void RestoreFromSnapshot(TSnapshot snapshot);
    bool NeedsSnapshot(int eventsCount);
}