using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using FlorianAlbert.FinanceObserver.Server.Startup;
using FlorianAlbert.FinanceObserver.Server.Startup.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Host.UseDefaultServiceProvider(serviceProviderOptions =>
{
    serviceProviderOptions.ValidateScopes = builder.Environment.IsDevelopment();
    serviceProviderOptions.ValidateOnBuild = true;
});

// Add services to the container.

builder.InstallServices(typeof(IServiceInstaller).Assembly);

builder.Services.AddAuthorization();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 4;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
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

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

app.MapIdentityApi<User>();

app.UseMigrations();

// Apply CORS before authorization/endpoints
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowLocalhost");
}

app.UseAuthorization();

app.MapControllers();

// Maps the OpenAPI endpoint for API documentation.
// The OpenAPI specification will be available at '/openapi/v1.json' (e.g., https://localhost:5001/openapi/v1.json).
app.MapOpenApi();

await app.RunAsync();