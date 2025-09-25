using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Resend;
using Serilog;
using Tienda_UCN_api.Infrastructure.Data;
using Tienda_UCN_api.src.API.Middlewares;
using Tienda_UCN_api.src.Application.Jobs;
using Tienda_UCN_api.src.Application.Jobs.Implements;
using Tienda_UCN_api.src.Application.Jobs.Interfaces;
using Tienda_UCN_api.src.Application.Mappers;
using Tienda_UCN_api.src.Application.Services.Implements;
using Tienda_UCN_api.src.Application.Services.Interfaces;
using Tienda_UCN_api.src.Domain.Models;
using Tienda_UCN_api.src.Infrastructure.Data;
using Tienda_UCN_api.src.Infrastructure.Middlewares;
using Tienda_UCN_api.src.Infrastructure.Repositories.Implements;
using Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

#region Database Connection
var connectionString = builder.Configuration.GetConnectionString("SqliteDatabase")
    ?? throw new InvalidOperationException("Connection string SqliteDatabase no configurado");
#endregion

#region Core Services
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region Mappers
builder.Services.AddScoped<ProductMapper>();
builder.Services.AddScoped<UserMapper>();
builder.Services.AddScoped<CartMapper>();
builder.Services.AddScoped<OrderMapper>();
#endregion

#region Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
#endregion

#region Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
#endregion

#region Jobs
builder.Services.AddScoped<IUserJob, UserJob>();
#endregion

#region Email Service
Log.Information("Configurando servicio de Email");
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["ResendAPIKey"]
        ?? throw new InvalidOperationException("El token de API de Resend no está configurado.");
});
builder.Services.AddTransient<IResend, ResendClient>();
#endregion

#region Authentication (JWT)
Log.Information("Configurando autenticación JWT");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    string jwtSecret = builder.Configuration["JWTSecret"]
        ?? throw new InvalidOperationException("La clave secreta JWT no está configurada.");
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtSecret)
        ),
        ValidateLifetime = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});
#endregion

#region Identity
Log.Information("Configurando Identity");
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = builder.Configuration["IdentityConfiguration:AllowedUserNameCharacters"]
        ?? throw new InvalidOperationException("Los caracteres permitidos para UserName no están configurados.");
})
.AddRoles<Role>()
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();
#endregion

#region Logging
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
#endregion

#region CORS
Log.Information("Configurando CORS");
try
{
    var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>()
        ?? throw new InvalidOperationException("Los orígenes permitidos CORS no están configurados.");
    var allowedMethods = builder.Configuration.GetSection("CORS:AllowedMethods").Get<string[]>()
        ?? throw new InvalidOperationException("Los métodos permitidos CORS no están configurados.");
    var allowedHeaders = builder.Configuration.GetSection("CORS:AllowedHeaders").Get<string[]>()
        ?? throw new InvalidOperationException("Los encabezados permitidos CORS no están configurados.");
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            policy => policy.WithOrigins(allowedOrigins)
                            .WithMethods(allowedMethods)
                            .WithHeaders(allowedHeaders));
    });
}
catch (Exception ex)
{
    Log.Error(ex, "Error al configurar CORS");
    throw;
}
#endregion

#region Database (SQLite)
Log.Information("Configurando base de datos SQLite");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteDatabase")));
#endregion

#region Hangfire
Log.Information("Configurando Hangfire");
var cronExpression = builder.Configuration["Jobs:CronJobDeleteUnconfirmedUsers"]
    ?? throw new InvalidOperationException("La expresión cron para eliminar usuarios no confirmados no está configurada.");
var timeZone = TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration["Jobs:TimeZone"]
    ?? throw new InvalidOperationException("La zona horaria para los trabajos no está configurada."));

builder.Services.AddHangfire(configuration =>
{
    var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);
    var databasePath = connectionStringBuilder.DataSource;

    configuration.UseSQLiteStorage(databasePath);
    configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
    configuration.UseSimpleAssemblyNameTypeSerializer();
    configuration.UseRecommendedSerializerSettings();
});
builder.Services.AddHangfireServer();
#endregion

#region Build App
var app = builder.Build();
#endregion

#region Hangfire Dashboard
app.UseHangfireDashboard(builder.Configuration["HangfireDashboard:DashboardPath"]
    ?? throw new InvalidOperationException("La ruta de hangfire no ha sido declarada"), new DashboardOptions
    {
        StatsPollingInterval = builder.Configuration.GetValue<int?>("HangfireDashboard:StatsPollingInterval")
        ?? throw new InvalidOperationException("El intervalo de actualización de estadísticas no está configurado."),
        DashboardTitle = builder.Configuration["HangfireDashboard:DashboardTitle"]
        ?? throw new InvalidOperationException("El título del panel de control de Hangfire no está configurado."),
        DisplayStorageConnectionString = builder.Configuration.GetValue<bool?>("HangfireDashboard:DisplayStorageConnectionString")
        ?? throw new InvalidOperationException("La configuración 'HangfireDashboard:DisplayStorageConnectionString' no está definida."),
    });
#endregion

#region Database Migration
Log.Information("Aplicando migraciones a la base de datos");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}
#endregion

#region Middleware Pipeline
Log.Information("Configurando el pipeline de la aplicación");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tienda UCN API V1");
    c.RoutePrefix = string.Empty;
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CartMiddleware>();

app.MapOpenApi();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
#endregion