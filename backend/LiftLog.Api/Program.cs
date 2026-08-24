using System.Text.Json.Serialization;
using FluentValidation;
using LiftLog.Api.Db;
using LiftLog.Api.Service;
using LiftLog.Api.Validators;
using LiftLog.Lib.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>(
    ServiceLifetime.Singleton
);

// Add services to the container.

builder.Services.AddDbContext<UserDataContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("UserDataContext"))
        .ReplaceService<IHistoryRepository, CamelCaseHistoryContext>()
        .UseSnakeCaseNamingConvention()
);
builder.Services.AddDbContext<RateLimitContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("RateLimitContext"))
        .ReplaceService<IHistoryRepository, CamelCaseHistoryContext>()
        .UseSnakeCaseNamingConvention()
);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddSingleton<PasswordService>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // The API is bound to loopback only, so every request arrives through the local Nginx proxy.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHostedService<CleanupExpiredDataHostedService>();
builder
    .Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.AllowTrailingCommas = true;
        opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opts.JsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter());
        opts.JsonSerializerOptions.Converters.Add(new ImmutableDictionaryJsonConverter());
    });

var app = builder.Build();
app.UseForwardedHeaders();
app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.MapMethods(
    "/health",
    ["GET", "HEAD"],
    () =>
    {
        return "healthy";
    }
);

if (!app.Configuration.GetValue<bool>("SkipDatabaseMigrations"))
{
    using var scope = app.Services.CreateScope();
    var userDb = scope.ServiceProvider.GetRequiredService<UserDataContext>();
    await userDb.Database.MigrateAsync();
    var rateLimitDb = scope.ServiceProvider.GetRequiredService<RateLimitContext>();
    await rateLimitDb.Database.MigrateAsync();
}

app.Run();
