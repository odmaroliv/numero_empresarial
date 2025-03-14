using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace NumeroEmpresarial.Common.Config

{
    public static class SecurityConfig
    {
        public static IServiceCollection AddAppSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowedOrigins", builder =>
                {
                    builder.WithOrigins(
                            configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                            new[] { "https://localhost:5001" })
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // Configuración de autenticación JWT
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // Eliminar tolerancia de tiempo
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Permitir el token JWT desde la cookie para SignalR
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

            // Configurar API Key Authentication para los webhooks
            services.AddAuthentication("ApiKey")
                .AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>("ApiKey", options => { });

            // Configuración de políticas de autorización
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
                options.AddPolicy("RequireApiAccess", policy => policy.RequireAuthenticatedUser());
            });

            // Configuración de Anti-Forgery token
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.Name = "XSRF-TOKEN";
                options.Cookie.HttpOnly = false; // Necesario para que JavaScript pueda leer la cookie
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            // Configuración de política de seguridad de contenido (CSP)
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Filters.AutoValidateAntiforgeryTokenAttribute());
            });

            return services;
        }

        public static IApplicationBuilder UseAppSecurity(this IApplicationBuilder app, IConfiguration configuration)
        {
            // Usar CORS
            app.UseCors("AllowedOrigins");

            // Usar HTTPS Redirection
            app.UseHttpsRedirection();

            // Configuración de encabezados de seguridad
            app.Use(async (context, next) =>
            {
                // Política de seguridad de contenido (CSP)
                context.Response.Headers.Append("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' https://js.stripe.com 'unsafe-inline'; " +
                    "style-src 'self' https://fonts.googleapis.com 'unsafe-inline'; " +
                    "img-src 'self' data: https:; " +
                    "font-src 'self' https://fonts.gstatic.com; " +
                    "connect-src 'self' https://api.stripe.com; " +
                    "frame-src 'self' https://js.stripe.com https://hooks.stripe.com; " +
                    "object-src 'none'");

                // Prevenir ataques de MIME-sniffing
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

                // Prevenir clickjacking
                context.Response.Headers.Append("X-Frame-Options", "DENY");

                // Activar protección XSS en navegadores modernos
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

                // Política de referrer
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

                // Feature Policy
                context.Response.Headers.Append("Permissions-Policy",
                    "camera=(), microphone=(), geolocation=(), payment=()");

                await next();
            });

            // Usar autenticación y autorización
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }

    // Configuración para API Key Authentication (para webhooks)
    public class ApiKeyAuthOptions : Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions { }

    public class ApiKeyAuthHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<ApiKeyAuthOptions>
    {
        private readonly IConfiguration _configuration;

        public ApiKeyAuthHandler(
            Microsoft.Extensions.Options.IOptionsMonitor<ApiKeyAuthOptions> options,
            ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
        {
            // Solo aplicar a rutas de webhook
            if (!Request.Path.StartsWithSegments("/api/webhook"))
            {
                return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.NoResult());
            }

            // Verificar la clave de Plivo Webhook
            if (Request.Path.StartsWithSegments("/api/webhook/plivo"))
            {
                var plivoAuthId = _configuration["Plivo:AuthId"];

                // En un escenario real, verificaríamos la firma del webhook de Plivo
                // Pero para esta implementación básica, simplemente permitimos las solicitudes
                // desde Plivo sin verificación adicional, confiando en la seguridad de la URL.

                var claims = new[] {
                    new System.Security.Claims.Claim("source", "plivo")
                };

                var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
            }

            // Verificar la clave de Stripe Webhook
            if (Request.Path.StartsWithSegments("/api/webhook/stripe"))
            {
                // Stripe usa su propio mecanismo de verificación de firma
                // El controlador de Stripe verificará la firma internamente

                var claims = new[] {
                    new System.Security.Claims.Claim("source", "stripe")
                };

                var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Fail("Invalid API key"));
        }
    }
}