

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NimbleArch.Api.Attributes;
using NimbleArch.SharedKernel.Validation.Abstract;
using NimbleArch.SharedKernel.Validation.Configuration;
using NimbleArch.SharedKernel.Validation.Exception;
using ValidationContext = NimbleArch.SharedKernel.Validation.Base.ValidationContext;
using ValidationResult = NimbleArch.SharedKernel.Validation.Result.ValidationResult;

namespace NimbleArch.Api.Middlewares;

/// <summary>
/// Middleware for handling validation in the API pipeline.
/// </summary>
/// <remarks>
/// EN: Provides automatic validation for incoming requests using the validation pipeline.
/// Handles validation errors and converts them to appropriate HTTP responses.
///
/// TR: Doğrulama pipeline'ını kullanarak gelen isteklerin otomatik doğrulamasını sağlar.
/// Doğrulama hatalarını işler ve uygun HTTP yanıtlarına dönüştürür.
/// </remarks>
public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationMiddleware> _logger;
    private readonly ValidationConfiguration _configuration;

    public ValidationMiddleware(
        RequestDelegate next,
        ILogger<ValidationMiddleware> logger,
        ValidationConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Processes the request through the validation pipeline.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            var validationContext = await CreateValidationContextAsync(context);
            var validationResult = await ValidateRequestAsync(context, validationContext);

            if (!validationResult.IsValid)
            {
                await HandleValidationErrorsAsync(context, validationResult);
                return;
            }

            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred");
            await HandleValidationExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Creates a validation context from the HTTP context.
    /// </summary>
    private async Task<ValidationContext> CreateValidationContextAsync(HttpContext context)
    {
        var builder = new ValidationContext.Builder()
            .WithUserId(context.User?.Identity?.Name)
            .WithEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            .WithServices(context.RequestServices);

        // Add tenant information if available
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tenantId))
        {
            builder.WithTenantId(tenantId);
        }

        return builder.Build();
    }

    /// <summary>
    /// Validates the incoming request.
    /// </summary>
    private async Task<ValidationResult> ValidateRequestAsync(
        HttpContext context,
        ValidationContext validationContext)
    {
        var endpoint = context.GetEndpoint();
        var validationAttributes = endpoint?
            .Metadata
            .OfType<ValidateAttribute>()
            .ToList();

        if (validationAttributes == null || !validationAttributes.Any())
        {
            return new ValidationResult(Array.Empty<ValidationError>());
        }

        using var cts = new CancellationTokenSource(_configuration.ValidationTimeoutMs);
        var errors = new List<ValidationError>();

        foreach (var attribute in validationAttributes)
        {
            var validatorType = attribute.ValidatorType;
            var validator = ActivatorUtilities.CreateInstance(
                context.RequestServices, 
                validatorType) as IValidator;

            if (validator == null)
            {
                continue;
            }

            // Get the request body if needed
            var model = await GetRequestModelAsync(context, attribute.ModelType);
            if (model == null)
            {
                continue;
            }

            var result = await validator.ValidateAsync(
                model, 
                validationContext,
                cts.Token);

            if (!result.IsValid)
            {
                errors.AddRange(result.Errors);

                if (_configuration.StopOnFirstFailure)
                {
                    break;
                }
            }
        }

        return new ValidationResult(errors);
    }

    /// <summary>
    /// Gets the model from request body.
    /// </summary>
    private async Task<object?> GetRequestModelAsync(HttpContext context, Type modelType)
    {
        if (!context.Request.HasJsonContentType())
        {
            return null;
        }

        try
        {
            return await JsonSerializer.DeserializeAsync(
                context.Request.Body,
                modelType,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize request body");
            return null;
        }
    }

    /// <summary>
    /// Handles validation errors by returning appropriate response.
    /// </summary>
    private async Task HandleValidationErrorsAsync(
        HttpContext context,
        ValidationResult validationResult)
    {
        var problemDetails = new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = context.Request.Path,
            Errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray())
        };

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problemDetails);
    }

    /// <summary>
    /// Handles validation exceptions.
    /// </summary>
    private async Task HandleValidationExceptionAsync(
        HttpContext context,
        ValidationException exception)
    {
        var problemDetails = new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation error occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = context.Request.Path,
            Detail = exception.Message
        };

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problemDetails);
    }
}