using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NumeroEmpresarial.Common.Config;
using NumeroEmpresarial.Components;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Core.Services;
using NumeroEmpresarial.Data;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de localizaci�n
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("es"),
        new CultureInfo("en")
    };

    options.DefaultRequestCulture = new RequestCulture("es");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// IMPORTANTE: Configurar PostgreSQL - SOLO UNA VEZ
// Eliminamos la duplicaci�n y nos quedamos con una sola configuraci�n de DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);

            npgsqlOptions.CommandTimeout(30);
            //npgsqlOptions.MigrationsAssembly("NumeroEmpresarial.Data");
        })
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);

// Agregar RazorComponents
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// IMPORTANTE: Configurar autenticaci�n SOLO UNA VEZ
// Configurar autenticaci�n con cookies y JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "MiClaveSecretaConAlMenos32CaracteresParaHMACSHA256")),
        ClockSkew = TimeSpan.Zero
    };
});

// Autorizaci�n
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
    options.AddPolicy("RequireApiAccess", policy => policy.RequireAuthenticatedUser());
});

// Agregar autenticaci�n para Blazor
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

// Servicios core
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPhoneNumberService, PhoneNumberService>();
builder.Services.AddScoped<IMessageWindowService, MessageWindowService>();
builder.Services.AddScoped<IPlivoService, PlivoService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Controladores para API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// IMPORTANTE: Modificamos para usar las optimizaciones pero SIN el DbContextPool
builder.Services.AddPerformanceOptimizationsWithoutDbContextPool(builder.Configuration);

// Configuraciones de seguridad
builder.Services.AddAppSecurity(builder.Configuration);

// CORS - Versi�n corregida
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policyBuilder =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

            if (allowedOrigins != null && allowedOrigins.Length > 0)
            {
                policyBuilder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
            else
            {
                // Configuraci�n por defecto si no hay or�genes configurados
                policyBuilder
                    .WithOrigins("https://localhost:5001")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        });
});

// Configuraci�n de anti-forgery para ASP.NET Core 8
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = false; // Necesario para JavaScript
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.FormFieldName = "__RequestVerificationToken";
});

// Servicios adicionales
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression();
builder.Services.AddResponseCaching();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Sembrar datos iniciales
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Asegurarse de que la base de datos est� creada y aplicar migraciones
        context.Database.Migrate();

        // Sembrar datos si es necesario
        // Nota: Aqu� verificamos si ya hay datos en lugar de comprobar migraciones pendientes
        if (!context.Users.Any())
        {
            SeedData.Initialize(context);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurri� un error al inicializar la base de datos.");
    }
}

// Usar optimizaciones configuradas
app.UsePerformanceOptimizations();

// Usar seguridad configurada
app.UseAppSecurity(builder.Configuration);

// Middleware b�sico
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");

// Autenticaci�n y autorizaci�n
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Mapeo de endpoints
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Middleware para capturar rutas no encontradas
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        context.Request.Path = "/Error";
        await next();
    }
});

app.Run();