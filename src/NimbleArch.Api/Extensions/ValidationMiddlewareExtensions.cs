using NimbleArch.Api.Middlewares;

namespace NimbleArch.Api.Extensions;

/// <summary>
/// Extension methods for adding validation middleware.
/// </summary>
public static class ValidationMiddlewareExtensions
{
    /// <summary>
    /// Adds validation middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseValidation(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ValidationMiddleware>();
    }
}