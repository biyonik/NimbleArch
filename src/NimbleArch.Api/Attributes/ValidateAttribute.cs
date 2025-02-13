namespace NimbleArch.Api.Attributes;

/// <summary>
/// Indicates that the endpoint requires validation.
/// </summary>
/// <remarks>
/// EN: Marks an endpoint for automatic validation using specified validator.
/// Can be applied to controller actions to enable validation middleware.
///
/// TR: Bir endpoint'i belirtilen doğrulayıcı kullanılarak otomatik doğrulama için işaretler.
/// Doğrulama middleware'ini etkinleştirmek için controller action'larına uygulanabilir.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class ValidateAttribute(Type validatorType, Type modelType) : Attribute
{
    /// <summary>
    /// Gets the type of validator to use.
    /// </summary>
    public Type ValidatorType { get; } = validatorType;

    /// <summary>
    /// Gets the type of model to validate.
    /// </summary>
    public Type ModelType { get; } = modelType;
}