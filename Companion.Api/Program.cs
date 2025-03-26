using Companion.Core.Services;
using Companion.Infrastructure.Configuration;
using Companion.Infrastructure.RabbitMq;
using Companion.Simulator.Hubs;
using Companion.Simulator.Services;
using Companion.Patterns.Services;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

// Configure RabbitMQ Management API client
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMqManagement"));
builder.Services.AddHttpClient<IRabbitMqManagementClient, RabbitMqManagementClient>();

// Configure RabbitMQ connection factory
builder.Services.AddSingleton<ConnectionFactory>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
    return new ConnectionFactory
    {
        HostName = settings.BaseUrl.Replace("http://", "").Replace("https://", "").Split(':')[0],
        UserName = settings.UserName,
        Password = settings.Password,
        VirtualHost = settings.VirtualHost
    };
});

// Register services
builder.Services.AddScoped<ITopologyService, TopologyService>();
builder.Services.AddScoped<IMessageFlowService, MessageFlowService>();
builder.Services.AddScoped<IPatternAnalysisService, PatternAnalysisService>();

// Add SignalR
builder.Services.AddSignalR();

// Add authorization
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RabbitMQ Developer's Companion API",
        Version = "v1",
        Description = "API for managing RabbitMQ topologies and simulations"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Include XML comments from referenced projects
    var simulatorXmlFile = "Companion.Simulator.xml";
    var simulatorXmlPath = Path.Combine(AppContext.BaseDirectory, simulatorXmlFile);
    if (File.Exists(simulatorXmlPath))
    {
        c.IncludeXmlComments(simulatorXmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RabbitMQ Developer's Companion API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map SignalR hubs
app.MapHub<SimulationHub>("/hubs/simulation");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
