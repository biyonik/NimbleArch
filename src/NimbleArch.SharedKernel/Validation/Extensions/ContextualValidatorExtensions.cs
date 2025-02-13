using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Services;

namespace NimbleArch.SharedKernel.Validation.Extensions;

/// <summary>
/// Provides extension methods for the ContextualValidator class.
/// </summary>
/// <remarks>
/// EN: Contains a collection of common validation rules that utilize context information.
/// These extensions provide a fluent API for adding context-aware validation rules to
/// validators. All methods are optimized for performance using expression compilation
/// and caching strategies.
///
/// TR: Bağlam bilgisini kullanan yaygın doğrulama kuralları koleksiyonunu içerir.
/// Bu uzantılar, doğrulayıcılara bağlama duyarlı doğrulama kuralları eklemek için
/// akıcı bir API sağlar. Tüm metodlar, ifade derleme ve önbellekleme stratejileri
/// kullanılarak performans için optimize edilmiştir.
/// </remarks>
public static class ContextualValidatorExtensions
{
    /// <summary>
    /// Validates that an entity belongs to the current tenant.
    /// </summary>
    /// <remarks>
    /// EN: Adds a tenant ownership validation rule. This rule ensures that the entity
    /// belongs to the tenant specified in the validation context. The validation is
    /// performed using compiled expressions for optimal performance.
    /// Uses Expression Trees to avoid runtime delegate compilation.
    ///
    /// TR: Kiracı sahipliği doğrulama kuralı ekler. Bu kural, varlığın doğrulama
    /// bağlamında belirtilen kiracıya ait olduğundan emin olur. Doğrulama, optimal
    /// performans için derlenmiş ifadeler kullanılarak gerçekleştirilir.
    /// Çalışma zamanı delegate derlemesinden kaçınmak için Expression Tree'ler kullanır.
    /// </remarks>
    /// <typeparam name="T">
    /// EN: The type of entity being validated
    /// TR: Doğrulanan varlığın tipi
    /// </typeparam>
    /// <param name="validator">
    /// EN: The validator instance to extend
    /// TR: Genişletilecek doğrulayıcı örneği
    /// </param>
    /// <param name="tenantIdExpression">
    /// EN: Expression to access the entity's tenant ID property
    /// TR: Varlığın kiracı ID özelliğine erişim için expression
    /// </param>
    /// <param name="group">
    /// EN: The validation group this rule belongs to
    /// TR: Bu kuralın ait olduğu doğrulama grubu
    /// </param>
    public static void BelongsToTenant<T>(
        this ContextualValidator<T> validator,
        Expression<Func<T, string>> tenantIdExpression,
        ValidationGroup group)
    {
        var memberName = GetMemberName(tenantIdExpression);
        
        validator.AddRule(
            (entity, context) => 
                tenantIdExpression.Compile()(entity) == context.TenantId,
            memberName,
            "Entity does not belong to the current tenant",
            group);
    }

    /// <summary>
    /// Validates that the current user has permission to modify the entity.
    /// </summary>
    /// <remarks>
    /// EN: Adds a permission validation rule. This rule checks if the current user
    /// has the required permissions to modify the entity. The validation utilizes
    /// the authorization service from the validation context.
    /// Supports role-based and permission-based authorization schemes.
    ///
    /// TR: İzin doğrulama kuralı ekler. Bu kural, mevcut kullanıcının varlığı
    /// değiştirmek için gerekli izinlere sahip olup olmadığını kontrol eder.
    /// Doğrulama, doğrulama bağlamından yetkilendirme servisini kullanır.
    /// Rol tabanlı ve izin tabanlı yetkilendirme şemalarını destekler.
    /// </remarks>
    /// <typeparam name="T">
    /// EN: The type of entity being validated
    /// TR: Doğrulanan varlığın tipi
    /// </typeparam>
    /// <param name="validator">
    /// EN: The validator instance to extend
    /// TR: Genişletilecek doğrulayıcı örneği
    /// </param>
    /// <param name="rolesExpression">
    /// EN: Expression to access the entity's required roles
    /// TR: Varlığın gerekli rollerine erişim için expression
    /// </param>
    /// <param name="group">
    /// EN: The validation group this rule belongs to
    /// TR: Bu kuralın ait olduğu doğrulama grubu
    /// </param>
    public static void HasPermission<T>(
        this ContextualValidator<T> validator,
        Expression<Func<T, IEnumerable<string>>> rolesExpression,
        ValidationGroup group)
    {
        var memberName = GetMemberName(rolesExpression);
        var compiledRoles = rolesExpression.Compile();

        validator.AddRule(
            Expression.Lambda<Func<T, ValidationContext, bool>>(
                Expression.Call(
                    typeof(ContextualValidatorExtensions).GetMethod(nameof(CheckPermission), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException(),
                    Expression.Constant(compiledRoles),
                    Expression.Parameter(typeof(T), "e"),
                    Expression.Parameter(typeof(ValidationContext), "c")
                ),
                Expression.Parameter(typeof(T), "e"),
                Expression.Parameter(typeof(ValidationContext), "c")
            ),
            memberName,
            "User does not have permission to modify this entity",
            group);
    }

    /// <summary>
    /// Extracts the member name from a property expression.
    /// </summary>
    /// <remarks>
    /// EN: Safely extracts the property name from an expression tree. This method
    /// handles member expressions and throws an appropriate exception if the
    /// expression is not a valid member access. Used internally by validation
    /// extension methods to get property names for error messages.
    ///
    /// TR: Bir expression tree'den özellik adını güvenli bir şekilde çıkarır.
    /// Bu metod, üye ifadelerini işler ve ifade geçerli bir üye erişimi değilse
    /// uygun bir istisna fırlatır. Hata mesajları için özellik adlarını almak
    /// üzere doğrulama uzantı metodları tarafından dahili olarak kullanılır.
    /// </remarks>
    /// <typeparam name="T">
    /// EN: The type containing the member
    /// TR: Üyeyi içeren tip
    /// </typeparam>
    /// <typeparam name="TProp">
    /// EN: The type of the member being accessed
    /// TR: Erişilen üyenin tipi
    /// </typeparam>
    /// <param name="expression">
    /// EN: The expression to extract the member name from
    /// TR: Üye adının çıkarılacağı ifade
    /// </param>
    /// <returns>
    /// EN: The name of the member
    /// TR: Üyenin adı
    /// </returns>
    /// <exception cref="ArgumentException">
    /// EN: Thrown when the expression is not a member access
    /// TR: İfade bir üye erişimi olmadığında fırlatılır
    /// </exception>
    private static string GetMemberName<T, TProp>(Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        throw new ArgumentException("Expression must be a member expression", nameof(expression));
    }
    
    /// <summary>
    /// Helper method to check permissions using the authorization service.
    /// </summary>
    private static bool CheckPermission<T>(
        Func<T, IEnumerable<string>> rolesAccessor,
        T entity,
        ValidationContext context)
    {
        var service = context.Services.GetService<IAuthorizationService>();
        if (service == null)
            return false;

        var roles = rolesAccessor(entity);
        return service.HasPermissionAsync(context.UserId, roles).GetAwaiter().GetResult();
    }
}