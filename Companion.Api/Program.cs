using Companion.Core.Services;
using Companion.Core.Repositories;
using Companion.Infrastructure.Repositories;
using Companion.Infrastructure.Configuration;
using Companion.Infrastructure.RabbitMq;
using Companion.Infrastructure.Services;
using Companion.Simulator.Hubs;
using Companion.Simulator.Services;
using Companion.Patterns.Services;
using Companion.Learning.Services;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Reflection;
using Companion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Companion.Core.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings!.SecretKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            ClockSkew = TimeSpan.Zero
        };

        // Configure for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireEditorRole", policy => policy.RequireRole("Admin", "Editor"));
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

// Configure PostgreSQL
builder.Services.AddDbContext<CompanionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITopologyService, TopologyService>();
builder.Services.AddScoped<ITopologyRepository, TopologyRepository>();
builder.Services.AddScoped<IMessageFlowService, MessageFlowService>();
builder.Services.AddScoped<IPatternAnalysisService, PatternAnalysisService>();
builder.Services.AddScoped<ILearningService, LearningService>();

// Add SignalR
builder.Services.AddSignalR();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:3000" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

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

    // Configure JWT authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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

// Use CORS before auth
app.UseCors();

// Add authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<SimulationHub>("/hubs/simulation");

// Apply any pending migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CompanionDbContext>();
    dbContext.Database.Migrate();
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
