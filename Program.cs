using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Configurar Host antes de qualquer outra configuração
builder.WebHost.UseUrls("http://localhost:5000");

// Configurar MassTransit com RabbitMQ e consumidor
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ResourceConsumer>(); // Registrar o consumidor
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ReceiveEndpoint("resource-queue", e =>
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
// app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Mantém as controllers funcionando
});


// Rota de Health Check manual (caso necessário)
// app.MapGet("/health", () => {
//     Console.WriteLine("🚀 Rota /health foi chamada!");
//     return Results.Ok(new { Status = true });
// });
app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/health"), appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"status\": true}");
    });
});

// Habilitar Logs de Debug para Ocelot
app.UseOcelot().Wait();

app.Run();
