using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Extensions;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Extensions;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Host.UseDefaultServiceProvider(serviceProviderOptions =>
{
    serviceProviderOptions.ValidateScopes = builder.Environment.IsDevelopment();
    serviceProviderOptions.ValidateOnBuild = true;
});

// Add services to the container.

builder.Services.AddAuthorization();

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: "finance-observer",
        options =>
        {
            options.Audience = "finance-observer-api";
            // OIDC best practice: only disable HTTPS metadata requirement in development
            if (builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    options.AddDocumentTransformer((document, context, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        // The OpenIdConnectUrl must be browser-accessible (not an internal service-discovery URL),
        // so we use the stable local port (8080) that Keycloak is bound to in AppHost/Program.cs.
        document.Components.SecuritySchemes["oidc"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OpenIdConnect,
            OpenIdConnectUrl = new Uri(
                "http://localhost:8080/realms/finance-observer/.well-known/openid-configuration")
        };
        return Task.CompletedTask;
    });
});

// CORS for local development (needed when Scalar proxy is disabled)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowLocalhost", policy =>
        {
            policy
                .SetIsOriginAllowed(origin =>
                {
                    if (string.IsNullOrWhiteSpace(origin))
                    {
                        return false;
                    }

                    return Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri) && uri.IsLoopback;
                })
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

builder.AddDataTransactionHandling();

builder.AddEntityFrameworkCoreDbAccess();

builder.AddFluentEmailManagement();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// Apply CORS before authorization/endpoints
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowLocalhost");
}

app.UseAuthentication();
app.UseAuthorization();

// Maps the OpenAPI endpoint for API documentation.
// The OpenAPI specification will be available at '/openapi/v1.json' (e.g., https://localhost:5001/openapi/v1.json).
app.MapOpenApi();

await app.RunAsync();