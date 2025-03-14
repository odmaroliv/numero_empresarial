using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NumeroEmpresarial.Components;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Core.Services;
using NumeroEmpresarial.Data;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración de localización
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

// Configurar PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Agregar RazorComponents
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar autenticación con cookies y JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
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

builder.Services.AddAuthorization();

// Agregar HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Agregar servicios de memoria caché
builder.Services.AddMemoryCache();

// Agregar soporte para controladores
builder.Services.AddControllers();

// Registrar servicios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPhoneNumberService, PhoneNumberService>();
builder.Services.AddScoped<IMessageWindowService, MessageWindowService>();
builder.Services.AddScoped<IPlivoService, PlivoService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

// Construir la aplicación
var app = builder.Build();

// Configurar el pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configurar localización
var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (locOptions != null)
{
    app.UseRequestLocalization(locOptions.Value);
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Mapear controladores API
app.MapControllers();

// Mapear endpoints de Razor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Mapear rutas para webhooks
app.MapControllerRoute(
    name: "webhooks",
    pattern: "api/webhook/{action=Index}/{id?}",
    defaults: new { controller = "Webhook" });

// Inicializar la base de datos
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        context.Database.Migrate();
        logger.LogInformation("Database migrated successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
    }
}

app.Run();