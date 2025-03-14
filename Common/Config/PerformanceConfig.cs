using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using NumeroEmpresarial.Data;
using System.IO.Compression;


namespace NumeroEmpresarial.Common.Config
{
    public static class PerformanceConfig
    {
        public static IServiceCollection AddPerformanceOptimizations(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de compresión de respuesta
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] {
                        "text/plain",
                        "text/css",
                        "application/javascript",
                        "text/html",
                        "application/xml",
                        "text/xml",
                        "application/json",
                        "text/json"
                    });
            });

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            // Configuración de caché distribuida
            //var useRedisCache = configuration.GetValue<bool>("Cache:UseRedis");

            //if (useRedisCache)
            //{
            //    services.AddStackExchangeRedisCache(options =>
            //    {
            //        options.Configuration = configuration.GetConnectionString("Redis");
            //        options.InstanceName = "NumeroEmpresarial:";
            //    });
            //}
            //else
            //{
            //    services.AddDistributedMemoryCache();
            //}

            // Configuración de caché de respuesta HTTP
            services.AddResponseCaching();

            // Configuración de Rate Limiting
            services.AddMemoryCache();

            services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                // Aumentar los límites para subidas de archivos (si es necesario)
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
            });

            // Configuración de DbContext pooling para mejorar rendimiento
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContextPool<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);

                    npgsqlOptions.CommandTimeout(30);
                    npgsqlOptions.MigrationsAssembly("NumeroEmpresarial.Data");
                });

                // Configuración de rendimiento para EF Core
                if (!configuration.GetValue<bool>("Database:EnableSensitiveDataLogging"))
                {
                    options.EnableSensitiveDataLogging(false);
                }

                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, poolSize: 128);

            return services;
        }

        public static IApplicationBuilder UsePerformanceOptimizations(this IApplicationBuilder app)
        {
            // Usar compresión de respuesta
            app.UseResponseCompression();

            // Usar caché de respuesta HTTP
            app.UseResponseCaching();

            // Middleware para habilitar caché de respuesta
            app.Use(async (context, next) =>
            {
                // Establecer encabezados de caché para recursos estáticos
                var path = context.Request.Path.Value;

                if (path.StartsWith("/css/") ||
                    path.StartsWith("/js/") ||
                    path.StartsWith("/lib/") ||
                    path.StartsWith("/images/"))
                {
                    // Caché recursos estáticos por 1 día
                    context.Response.Headers.Add("Cache-Control", "public,max-age=86400");
                    context.Response.Headers.Add("Expires", DateTime.UtcNow.AddDays(1).ToString("R"));
                }
                else if (!path.StartsWith("/api/"))
                {
                    // Caché páginas por 1 hora (excepto API)
                    context.Response.Headers.Add("Cache-Control", "public,max-age=3600");
                    context.Response.Headers.Add("Expires", DateTime.UtcNow.AddHours(1).ToString("R"));
                }
                else
                {
                    // No cachear respuestas de API por defecto
                    context.Response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
                    context.Response.Headers.Add("Pragma", "no-cache");
                    context.Response.Headers.Add("Expires", "0");
                }

                await next();
            });

            return app;
        }
    }
}