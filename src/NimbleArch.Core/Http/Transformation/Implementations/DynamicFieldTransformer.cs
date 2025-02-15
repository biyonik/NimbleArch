using System.Linq.Expressions;

namespace NimbleArch.Core.Http.Transformation.Implementations;

/// <summary>
/// Dynamic implementation of field transformer using IL emission.
/// </summary>
public class DynamicFieldTransformer<T> : IFieldTransformer<T>
{
    private readonly Func<T, T> _transformDelegate;
    private readonly HashSet<string> _includedFields;

    public IReadOnlySet<string> IncludedFields => _includedFields;

    public DynamicFieldTransformer(HashSet<string> fields)
    {
        _includedFields = fields;
        _transformDelegate = CreateTransformDelegate();
    }

    public T Transform(T source)
    {
        return _transformDelegate(source);
    }

    private Func<T, T> CreateTransformDelegate()
    {
        var sourceType = typeof(T);
        var properties = sourceType.GetProperties()
            .Where(p => _includedFields.Contains(p.Name))
            .ToList();

        // Parameter için expression oluştur
        var parameter = Expression.Parameter(sourceType, "source");

        // Yeni nesne oluşturma expression'ı
        var bindings = properties.Select(prop =>
            Expression.Bind(
                prop,
                Expression.Property(parameter, prop)
            )).ToList();

        // MemberInit expression'ı oluştur
        var init = Expression.MemberInit(
            Expression.New(sourceType),
            bindings);

        // Lambda oluştur ve derle
        var lambda = Expression.Lambda<Func<T, T>>(init, parameter);
        return lambda.Compile();
    }
}