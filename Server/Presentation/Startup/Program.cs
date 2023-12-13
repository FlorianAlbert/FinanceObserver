using FlorianAlbert.FinanceObserver.Server.Startup;
using FlorianAlbert.FinanceObserver.Server.Startup.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(serviceProviderOptions =>
{
    serviceProviderOptions.ValidateScopes = builder.Environment.IsDevelopment();
    serviceProviderOptions.ValidateOnBuild = true;
});

// Add services to the container.

builder.Services
    .InstallServices(builder.Configuration, typeof(IServiceInstaller).Assembly);

var app = builder.Build();

app.UseMigrations();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();