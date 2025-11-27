using API;
using API.Endpoints;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();

try
{
    Log.Information("Starting Congestion Tax Calculator API");

    // Add services to the container.
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddApiServices();

    var app = builder.Build();

    // Seed database
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<CongestionTaxDbContext>();
        await context.Database.EnsureCreatedAsync();
        //await context.Database.MigrateAsync();
        await SeedData.SeedGothenburg2013Async(context);
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();

    // Configure middleware pipeline
    app.ConfigureMiddleware();

    // Redirect root to Scalar API documentation
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

    // Map API endpoints
    app.MapTaxEndpoints();
    app.MapCityEndpoints();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
