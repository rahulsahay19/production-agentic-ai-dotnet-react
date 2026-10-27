using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using NexaStore.Api.Configuration;
using NexaStore.Api.Infrastructure;
using NexaStore.Application;
using NexaStore.Application.Configuration;
using NexaStore.Infrastructure;
using NexaStore.Infrastructure.Persistence;
using NexaStore.Infrastructure.Persistence.Seeding;
using NexaStore.Infrastructure.AI;
using NexaStore.Infrastructure.Knowledge;
using NexaStore.Infrastructure.Mcp;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

var connectionString = builder.Configuration.GetConnectionString("NexaStore");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = "Host=localhost;Port=5432;Database=nexastore";
}

var commerceOptions = builder.Configuration.GetSection(CommerceOptions.SectionName).Get<CommerceOptions>() ?? new();
builder.Services.AddApplication(commerceOptions);
builder.Services.AddInfrastructure(connectionString);
var aiOptions = builder.Configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();
builder.Services.AddAi(aiOptions);
var knowledgeOptions = builder.Configuration.GetSection(KnowledgeOptions.SectionName).Get<KnowledgeOptions>() ?? new();
builder.Services.AddKnowledge(knowledgeOptions, aiOptions);
var mcpOptions = builder.Configuration.GetSection(McpOptions.SectionName).Get<McpOptions>() ?? new();
builder.Services.AddMcpToolDiscovery(mcpOptions);
if(mcpOptions.Enabled)
{
    builder.Services.AddMcpServer()
        .WithHttpTransport()
        .WithTools<NexaStoreMcpTools>()
        .WithResources<NexaStoreMcpResources>();
}
builder.Services.Configure<FrontendOptions>(builder.Configuration.GetSection(FrontendOptions.SectionName));
var frontendOptions = builder.Configuration.GetSection(FrontendOptions.SectionName).Get<FrontendOptions>() ?? new();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    if (frontendOptions.AllowedOrigins.Length > 0)
    {
        policy.WithOrigins(frontendOptions.AllowedOrigins).AllowAnyHeader().AllowAnyMethod();
    }
}));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
});
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<ValidationFilter>());
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NexaStore API",
        Version = "v1",
        Description = "Conventional e-commerce API for developers, creators, and AI builders."
    });
});
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NexaStoreDbContext>("postgresql", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Handled {RequestMethod} {RequestPath} with {StatusCode} in {Elapsed:0.0000} ms";
});
app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "NexaStore API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthorization();
app.MapControllers();
if(mcpOptions.Enabled)
{
    app.MapMcp("/mcp");
}
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapHealthChecks("/health");

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NexaStoreDbContext>();
    await dbContext.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync();
}

await app.RunAsync();

public partial class Program;
