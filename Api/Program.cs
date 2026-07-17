using System.Text;
using Api.Auth;
using Api.Common;
using Api.Swagger;
using Api.Tenants;
using Application.Common;
using Application.Tenants;
using Infrastructure;
using Infrastructure.Identity;
using Infrastructure.Regions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCors";

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("Default");
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    allowedOrigins =
    [
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "http://localhost:5174",
        "http://127.0.0.1:5174"
    ];
}

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ActiveTenantAuthorizationHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();
builder.Services.AddScoped<IRequestContext,HttpRequestContext>();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
    });
});
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.MapInboundClaims = false;
        var secret = builder.Configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is required.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT_ISSUER"],
            ValidAudience = builder.Configuration["JWT_AUDIENCE"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ActiveTenantPolicy.Name, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new ActiveTenantRequirement());
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Dialysis API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Description = "Paste only the JWT access token. Do not include 'Bearer '."
    });

    options.OperationFilter<AuthorizeCheckOperationFilter>();
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks().AddNpgSql(connectionString!);
//builder.Services.AddOpenApi();

var app = builder.Build();
var seedIdentityEnabled = app.Configuration.GetValue<bool>("SEED_IDENTITY_ENABLED");

await RegionSeeder.SeedAsync(app.Services);

if (seedIdentityEnabled)
{
    await IdentitySeeder.SeedAsync(app.Services, app.Configuration);
}
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);

//app.UseAuthorization();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
