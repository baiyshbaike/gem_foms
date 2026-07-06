 using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Dialysis.Server.Domain;
using Microsoft.AspNetCore.Identity;
using Duende.IdentityServer.Services;
using Dialysis.Server.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using DevExpress.AspNetCore.Reporting;
using DevExpress.AspNetCore;
using DevExpress.XtraReports.Web.Extensions;
using Dialysis.Server.Data;
using DevExpress.Security.Resources;
using NLog;
using DevExpress.XtraCharts;
using Dialysis.Client;
using Dialysis.Server;
using Dialysis.Server.Hubs;
using Microsoft.AspNetCore.RateLimiting;


using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);


//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
//GlobalDiagnosticsContext.Set("configDir", "D:\\AWORKS\\Logs");
GlobalDiagnosticsContext.Set("connectionString", builder.Configuration.GetConnectionString("DefaultConnection"));


// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDevExpressControls();
builder.Services.AddScoped<ReportStorageWebExtension, CustomReportStorageWebExtension>();
builder.Services.ConfigureReportingServices(configurator => {
    configurator.ConfigureReportDesigner(designerConfigurator => {
    });
    configurator.ConfigureWebDocumentViewer(viewerConfigurator => {
        viewerConfigurator.UseCachedReportSourceBuilder();
        viewerConfigurator.RegisterConnectionProviderFactory<CustomSqlDataConnectionProviderFactory>();
    });
    configurator.UseAsyncEngine();
});
builder.Services.AddDbContext<ReportDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("ReportsDataConnectionString")), ServiceLifetime.Scoped);


/*builder.Services.AddDefaultIdentity()
               .AddRoles<AppRole>()
               .AddEntityFrameworkStores<AppDbContext>()
               .AddDefaultTokenProviders();*/

/*/*builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>()    
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();*/
//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

//builder.Services.AddIdentityServer();

builder.Services.AddAuthentication(
    authentication =>
    {
        authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(bearer =>
    {
        bearer.RequireHttpsMetadata = false;
        bearer.SaveToken = true;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppConfiguration:Secret"] ?? "DEFAULT_SECRET_PLEASE_CHANGE_IN_PRODUCTION")),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
        bearer.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirstValue("jti");
                if (!string.IsNullOrEmpty(jti))
                {
                    var blacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
                    if (await blacklistService.IsTokenBlacklistedAsync(jti))
                    {
                        context.Fail("Token has been revoked.");
                        return;
                    }
                }

                var tokenVersionClaim = context.Principal?.FindFirstValue("tokenVersion");
                var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(tokenVersionClaim) && !string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var userId))
                {
                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                    var user = await dbContext.User.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                    if (user == null || user.TokenVersion.ToString() != tokenVersionClaim)
                    {
                        context.Fail("Token version mismatch. Please login again.");
                    }
                }
            }
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(policyName: "login", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

//builder.Services.AddAuthentication()
//    .AddIdentityServerJwt();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IActiveUserService, ActiveUserService>();

builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<IUserProfileService, UserProfileService>();
builder.Services.AddTransient<IRegionService, RegionService>();
builder.Services.AddTransient<IStatusService, StatusService>();
builder.Services.AddTransient<IPatientService, PatientService>();
builder.Services.AddTransient<IHDSessionService, HDSessionService>();
builder.Services.AddTransient<IMedCenterService, MedCenterService>();
builder.Services.AddTransient<IMedCardService, MedCardService>();
builder.Services.AddTransient<IReportService, ReportService>();
builder.Services.AddTransient<IExcellService, ExcellService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddSingleton<ILoggerManager, LoggerManager>();
builder.Services.AddSingleton<ILogSettingsProvider, LogSettingsProvider>();
builder.Services.AddHostedService<LogCleanupService>();
builder.Services.AddScoped<IHttpLogService, HttpLogService>();
builder.Services.AddScoped<IActionLogService, ActionLogService>();
builder.Services.AddTransient<IVerifyService, VerifyService>();
builder.Services.AddHostedService<BgService>();

builder.Services.ConfigureReportingServices(configurator => {
    configurator.ConfigureReportDesigner(designerConfigurator => {
        designerConfigurator.EnableCustomSql();
    });
});

//builder.Services.ConfigureLoggerService();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FOMS Dializ API", Version = "v1" });
    c.CustomSchemaIds(type => type.FullName);
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.WebHost.UseStaticWebAssets();
builder.Services.AddSignalR();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Clear known networks and proxies since we are usually behind a reverse proxy
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

//var db = app.Services.GetService<ReportDbContext>();
//db.InitializeDatabase();

var contentDirectoryAllowRule = DirectoryAccessRule.Allow(new DirectoryInfo(Path.Combine(app.Environment.ContentRootPath, "..", "Content")).FullName);
AccessSettings.ReportingSpecificResources.TrySetRules(contentDirectoryAllowRule, UrlAccessRule.Allow());
app.UseReporting(builder => {
    builder.UserDesignerOptions.DataBindingMode = DevExpress.XtraReports.UI.DataBindingMode.Expressions;   
});


app.UseForwardedHeaders();

//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())

{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseMiddleware<ExceptionMiddleware>();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors(cors => cors
    .WithOrigins(builder.Configuration["AppConfiguration:ApplicationUrl"] ?? "http://localhost:5000") // Restrict to specific origins
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
);

app.UseRateLimiter();
// Note: UseAntiforgery() is .NET 8+. Since this API uses JWT (Authorization header),
// CSRF tokens are not needed — browser cookie-based attacks do not apply.



app.UseRouting();

//app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<IpValidationMiddleware>(); // Token IP binding check
app.UseMiddleware<HttpLoggingMiddleware>(); // HTTP request/response logging


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FOMS Dializ API V1");
    });
}
app.MapHub<VerificationHub>("/verificationHub");
app.Run();

