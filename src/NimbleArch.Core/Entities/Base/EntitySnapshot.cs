namespace NimbleArch.Core.Entities.Base;

/// <summary>
/// Represents a snapshot of an entity's state.
/// </summary>
public record EntitySnapshot<TKey> where TKey : struct
{
    public TKey Id { get; set; }
    public long Version { get; set; }
    public EntityState State { get; set; }
    public List<string> ModifiedProperties { get; set; } = new();
    public List<IDomainEvent> Events { get; set; } = new();
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}