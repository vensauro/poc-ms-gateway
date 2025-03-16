using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MassTransit;
using PocMsGateway.Messaging;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// builder.Environment.IsDevelopment()
string serverHost = builder.Configuration["Server:Host"];
string rabbitmqHost = builder.Configuration["RabbitMQ:Host"];
string rabbitmqUser = builder.Configuration["RabbitMQ:Username"];
string rabbitmqPassword = builder.Configuration["RabbitMQ:Password"];

// Configurar Host antes de qualquer outra configuração
builder.WebHost.UseUrls(serverHost);

// Configurar MassTransit com RabbitMQ e consumidor
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ResourceConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitmqHost, h =>
        {
            h.Username(rabbitmqUser);
            h.Password(rabbitmqPassword);
        });
        cfg.ReceiveEndpoint("input-queue", e =>
        {
            e.ConfigureConsumer<ResourceConsumer>(context);
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

app.Run();
