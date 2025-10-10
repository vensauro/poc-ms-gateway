using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MassTransit;
using PocMsGateway.Messaging;
using PocMsGateway.DTOs;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// builder.Environment.IsDevelopment()
string serverHost = builder.Configuration["Server:Host"];
string rabbitmqHost = builder.Configuration["RabbitMQ:Host"];
string rabbitmqUser = builder.Configuration["RabbitMQ:Username"];
string rabbitmqPassword = builder.Configuration["RabbitMQ:Password"];

// Configurar Host antes de qualquer outra configuração
// builder.WebHost.UseUrls(serverHost);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Configurar MassTransit com RabbitMQ e consumidor
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ResourceConsumer<TaskCreatedData>>();
    x.AddConsumer<ResourceConsumer<ListTaskData>>();
    x.AddConsumer<ResourceConsumer<TaskGetPayload>>();
    x.AddConsumer<ResourceConsumer<TaskDeletePayload>>();
    x.AddConsumer<ResourceConsumer<NotificationData>>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h =>
        {
            h.Username(rabbitmqUser);
            h.Password(rabbitmqPassword);
        });
        cfg.ReceiveEndpoint("task_queue", e =>
        {
            e.ConfigureConsumer<ResourceConsumer<TaskCreatedData>>(context);
            e.ConfigureConsumer<ResourceConsumer<ListTaskData>>(context);
            e.ConfigureConsumer<ResourceConsumer<TaskGetPayload>>(context);
            e.ConfigureConsumer<ResourceConsumer<TaskDeletePayload>>(context);
        });
        cfg.ReceiveEndpoint("notification_queue", e =>
        {
            e.ConfigureConsumer<ResourceConsumer<NotificationData>>(context);
        });
    });
});

// Configurar Ocelot
builder.Services.AddOcelot();
builder.Configuration.AddOcelot(
    builder.Environment.IsDevelopment()
        ? Path.GetFullPath(@"Config/Ocelot/Development")
        : Path.GetFullPath(@"Config/Ocelot/Build"),
    builder.Environment
);

// Configurar Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddTransient<IMessagePublisher, MessagePublisher>();
builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();

var app = builder.Build();

app.UseRouting(); // Habilita o roteamento padrão

// Configurar Middleware e Rotas
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Garantir que as Controllers sejam carregadas antes do Ocelot
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Rota de Health Check manual (caso necessário)
app.MapGet("/health", () => {
    Console.WriteLine("🚀 Rota /health foi chamada!");
    return Results.Ok(new { Status = true });
});

// Habilitar Logs de Debug para Ocelot
app.UseOcelot().Wait();

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

app.Run();
