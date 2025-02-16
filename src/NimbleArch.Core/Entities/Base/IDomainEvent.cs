namespace NimbleArch.Core.Entities.Base;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    long Version { get; }
}