﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FastEndpoints.Swagger;

/// <summary>
/// a set of extension methods for adding swagger support
/// </summary>
public static class Extensions
{
    /// <summary>
    /// enable support for FastEndpoints in swagger
    /// </summary>
    public static void EnableFastEndpoints(this SwaggerGenOptions options)
    {
        options.CustomSchemaIds(type => type.FullName);
        options.TagActionsBy(d => new[] { d.RelativePath?.Split('/')[0] });
        options.OperationFilter<DefaultOperationFilter>();
    }

    /// <summary>
    /// enable jwt bearer authorization support
    /// </summary>
    public static void EnableJWTBearerAuth(this SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter a JWT token to authorize the requests..."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "BearerAuth"
                    }
                },
                Array.Empty<string>()
            }
        });
    }

    /// <summary>
    /// enable swagger support for FastEndpoints with a single call.
    /// </summary>
    /// <param name="options">swaggergen config options</param>
    /// <param name="serializerOptions">json serializer options</param>
    /// <param name="addJWTBearerAuth">set to false to disable auto addition of jwt bearer auth support</param>
    public static IServiceCollection AddSwagger(this IServiceCollection services,
        Action<SwaggerGenOptions>? options = null,
        Action<JsonOptions>? serializerOptions = null,
        bool addJWTBearerAuth = true)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(o =>
        {
            o.EnableFastEndpoints();
            if (addJWTBearerAuth) o.EnableJWTBearerAuth();
            options?.Invoke(o);
        });

        if (serializerOptions is not null)
            services.AddMvcCore().AddJsonOptions(serializerOptions);
        else
            services.AddMvcCore().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

        return services;
    }
}