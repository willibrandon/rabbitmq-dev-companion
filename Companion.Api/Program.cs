using Companion.Core.Services;
using Companion.Infrastructure.Configuration;
using Companion.Infrastructure.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

// Configure RabbitMQ Management API client
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMqManagement"));
builder.Services.AddHttpClient<IRabbitMqManagementClient, RabbitMqManagementClient>();

// Register services
builder.Services.AddScoped<ITopologyService, TopologyService>();

// Add authorization
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RabbitMQ Developer's Companion API", Version = "v1" });
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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
