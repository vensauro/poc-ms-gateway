using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MassTransit;
using PocMsGateway.Messaging;
using PocMsGateway.DTOs;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ApiGateway.BuildingBlocks.AccessControl.ApiAuth;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtContext, JwtContext>();

string serverHost = builder.Configuration["Server:Host"];
string rabbitmqHost = builder.Configuration["RabbitMQ:Host"];
string rabbitmqUser = builder.Configuration["RabbitMQ:Username"];
string rabbitmqPassword = builder.Configuration["RabbitMQ:Password"];

Console.WriteLine(rabbitmqHost);
Console.WriteLine(serverHost);

builder.WebHost.UseUrls(serverHost);

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h =>
        {
            h.Username(rabbitmqUser);
            h.Password(rabbitmqPassword);
        });
        cfg.ReceiveEndpoint("gateway_queue", e =>
        {});
    });
});

builder.Services.Configure<ApiKeySettings>(
    builder.Configuration.GetSection("ApiKeySettings")
);
builder.Services.Configure<ApiAuthAuthenticationOptions>(
    builder.Configuration.GetSection("Jwt")
);

builder.Services.AddTransient<ApiScopeHandler>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = ApiAuthAuthenticationOptions.DefaultScheme;
        options.DefaultChallengeScheme = ApiAuthAuthenticationOptions.DefaultScheme;
    })
    .AddApiAuthSupport(builder.Configuration)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? "placeholder"))
        };
    });

builder.Services.AddAuthorization();

builder.Configuration.AddOcelot(
    builder.Environment.IsDevelopment()
        ? Path.GetFullPath(@"Config/Ocelot/Development")
        : Path.GetFullPath(@"Config/Ocelot/Build"),
    builder.Environment
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services
    .AddOcelot(builder.Configuration);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AsyncAPI
builder.Services.AddAsyncApiSchemaGeneration(options =>
{
    options.AsyncApi = new AsyncApiDocument
    {
        Info = new Info("PocMsGateway", "1.0.0")
        {
            Description = "Documentação AsyncAPI do Gateway de Mensageria"
        },
        Servers =
        {
            ["rabbitmq"] = new Server("amqp://rabbitmq", "amqp")
            {
                Description = "Servidor RabbitMQ principal"
            }
        }
    };
    // Rotas de documentação
    options.Middleware.Route = "/docs/asyncapi";
    options.Middleware.UiBaseRoute = "/docs/asyncapi/ui/";
    options.Middleware.UiTitle = "AsyncAPI - Gateway de Mensageria";
});

// Serviços personalizados
// builder.Services.AddTransient<IMessagePublisher, MessagePublisher>();
builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();

var app = builder.Build();

app.UseRouting(); // Habilita o roteamento padrão

app.UseCors("AllowFlutter");

// Configurar Middleware e Rotas
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PocMsGateway API v1");
        c.RoutePrefix = "swagger";

        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // inicia recolhido
        c.DisplayRequestDuration(); // mostra tempo das requisições
        c.DefaultModelsExpandDepth(-1); // oculta o painel Models
        c.DocumentTitle = "Ocelot Gateway - Swagger UI";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

// Rotas carreagas antes do Ocelot
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapAsyncApiDocuments(); // /docs/asyncapi
    endpoints.MapAsyncApiUi();        // /docs/asyncapi/ui
});

// Rota de Health Check manual
app.MapGet("/health", () =>
{
    Console.WriteLine("🚀 Rota /health foi chamada!");
    return Results.Ok(new { Status = true });
});

// Rota de Swagger
app.MapGet("/swagger/v1/swagger.json", () =>
{
    var envName = app.Environment.IsDevelopment() ? "Development" : "Build";
    var ocelotDir = Path.Combine(Directory.GetCurrentDirectory(), "Config", "Ocelot", envName);

    if (!Directory.Exists(ocelotDir))
        return Results.Problem($"Diretório não encontrado: {ocelotDir}");

    var routeFiles = Directory.GetFiles(ocelotDir, "ocelot.*.json")
        .Where(f => !f.Contains("global", StringComparison.OrdinalIgnoreCase))
        .ToList();

    var allRoutes = new List<JsonObject>();

    foreach (var file in routeFiles)
    {
        var json = JsonNode.Parse(File.ReadAllText(file));
        var routes = json?["Routes"]?.AsArray();
        if (routes != null)
        {
            foreach (var route in routes)
            {
                if (route is JsonObject obj)
                    allRoutes.Add(obj);
            }
        }
    }

    var paths = new JsonObject();
    foreach (var route in allRoutes)
    {
        var upstreamPath = route["UpstreamPathTemplate"]?.ToString();
        var methods = route["UpstreamHttpMethod"]?.AsArray()?.Select(m => m.ToString().ToLowerInvariant()).ToList()
                      ?? new List<string> { "get" };

        if (string.IsNullOrEmpty(upstreamPath))
            continue;

        var pathItem = new JsonObject();

        foreach (var method in methods)
        {
            pathItem[method] = new JsonObject
            {
                ["summary"] = $"Proxy for {upstreamPath}",
                ["responses"] = new JsonObject
                {
                    ["200"] = new JsonObject { ["description"] = "OK" }
                }
            };
        }

        paths[upstreamPath] = pathItem;
    }

    var swagger = new JsonObject
    {
        ["openapi"] = "3.0.1",
        ["info"] = new JsonObject
        {
            ["title"] = "Ocelot Gateway",
            ["version"] = "1.0.0",
            ["description"] = "Swagger dinâmico gerado a partir das rotas do Ocelot"
        },
        ["paths"] = paths
    };

    return Results.Text(
        JsonSerializer.Serialize(swagger, new JsonSerializerOptions { WriteIndented = true }),
        "application/json",
        Encoding.UTF8
    );
});

// Redirect for docs
app.MapGet("/docs/swagger", ctx =>
{
    ctx.Response.Redirect("/docs/swagger/index.html");
    return Task.CompletedTask;
});

// Global error catcher
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Exception capturada: {ex}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Erro interno detectado. Veja logs.");
    }
});

// Console.WriteLine("Assemblies carregados:");
// foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
// {
//     Console.WriteLine($"- {asm.FullName}");
// }

await app.UseOcelot();

app.Run();
