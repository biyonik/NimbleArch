using NimbleArch.SharedKernel.Validation.Interfaces;
using NimbleArch.SharedKernel.Validation.Models;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Services.Impl;

/// <summary>
/// Basic implementation of the data consistency checker.
/// </summary>
public class BasicDataConsistencyChecker : IDataConsistencyChecker
{
    public async Task<ConsistencyCheckResult> CheckConsistencyAsync<T>(
        T entity,
        CancellationToken cancellationToken = default) where T : IHasRelations
    {
        var inconsistencies = new List<Inconsistency>();

        foreach (var relation in entity.Relations)
        {
            // In a real implementation, we would check each relationship
            // For now, we'll just verify that foreign key properties are not null
            var foreignKeyValue = GetPropertyValue(entity, relation.ForeignKeyProperty);
            if (foreignKeyValue == null && relation.RelationType != RelationType.OneToMany)
            {
                inconsistencies.Add(new Inconsistency(
                    entity.GetType().Name,
                    $"Required foreign key {relation.ForeignKeyProperty} is null"));
            }
        }

        return new ConsistencyCheckResult(inconsistencies.Count == 0, inconsistencies);
    }

    private object GetPropertyValue(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
    }
}