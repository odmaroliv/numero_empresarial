using Microsoft.EntityFrameworkCore;
using NumeroEmpresarial.Domain.Entities;

namespace NumeroEmpresarial.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PhoneNumber> PhoneNumbers { get; set; }
        public DbSet<MessageWindow> MessageWindows { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<LanguageConfiguration> LanguageConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de las entidades

            // Usuario
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("nombre").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasColumnName("telefono").IsRequired().HasMaxLength(20);
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
                entity.Property(e => e.RegistrationDate).HasColumnName("fecha_registro").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.LastLogin).HasColumnName("ultimo_acceso").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ApiKey).HasColumnName("api_key").HasMaxLength(100);
                entity.Property(e => e.Active).HasColumnName("activo").HasDefaultValue(true);
                entity.Property(e => e.Balance).HasColumnName("saldo").HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(e => e.Language).HasColumnName("idioma").HasMaxLength(10).HasDefaultValue("es");
                entity.Property(e => e.RefreshToken).HasColumnName("refresh_token").HasMaxLength(255);
                entity.Property(e => e.RefreshTokenExpiryTime).HasColumnName("refresh_token_expiry_time");

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasMany(u => u.PhoneNumbers)
                      .WithOne(p => p.User)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Número de teléfono
            modelBuilder.Entity<PhoneNumber>(entity =>
            {
                entity.ToTable("numeros_telefono");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("usuario_id");
                entity.Property(e => e.Number).HasColumnName("numero").IsRequired().HasMaxLength(20);
                entity.Property(e => e.PlivoId).HasColumnName("plivo_id").IsRequired().HasMaxLength(100);
                entity.Property(e => e.AcquisitionDate).HasColumnName("fecha_adquisicion").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ExpirationDate).HasColumnName("fecha_expiracion");
                entity.Property(e => e.Active).HasColumnName("activo").HasDefaultValue(true);
                entity.Property(e => e.RedirectionNumber).HasColumnName("numero_redireccion").IsRequired().HasMaxLength(20);
                entity.Property(e => e.Type).HasColumnName("tipo_numero").HasDefaultValue(0);
                entity.Property(e => e.MonthlyCost).HasColumnName("costo_por_mes").HasColumnType("decimal(10,2)");

                entity.HasIndex(e => e.Number).IsUnique();
                entity.HasMany(p => p.MessageWindows)
                      .WithOne(m => m.PhoneNumber)
                      .HasForeignKey(m => m.PhoneNumberId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Ventana de mensajes
            modelBuilder.Entity<MessageWindow>(entity =>
            {
                entity.ToTable("ventanas_mensaje");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.PhoneNumberId).HasColumnName("numero_telefono_id");
                entity.Property(e => e.StartTime).HasColumnName("fecha_inicio").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.EndTime).HasColumnName("fecha_fin");
                entity.Property(e => e.Active).HasColumnName("activa").HasDefaultValue(true);
                entity.Property(e => e.MaxMessages).HasColumnName("numero_maximo_mensajes").HasDefaultValue(10);
                entity.Property(e => e.ReceivedMessages).HasColumnName("mensajes_recibidos").HasDefaultValue(0);
                entity.Property(e => e.WindowCost).HasColumnName("costo_por_ventana").HasColumnType("decimal(10,2)");

                entity.HasMany(w => w.Messages)
                      .WithOne(m => m.MessageWindow)
                      .HasForeignKey(m => m.MessageWindowId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Mensaje
            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("mensajes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.MessageWindowId).HasColumnName("ventana_mensaje_id");
                entity.Property(e => e.From).HasColumnName("de").IsRequired().HasMaxLength(20);
                entity.Property(e => e.Text).HasColumnName("texto").IsRequired();
                entity.Property(e => e.ReceivedTime).HasColumnName("fecha_recepcion").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Redirected).HasColumnName("redirigido").HasDefaultValue(false);
                entity.Property(e => e.MessageCost).HasColumnName("costo_mensaje").HasColumnType("decimal(10,2)");
            });

            // Transacción
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transacciones");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("usuario_id");
                entity.Property(e => e.StripeId).HasColumnName("stripe_id").HasMaxLength(100);
                entity.Property(e => e.Amount).HasColumnName("monto").HasColumnType("decimal(10,2)");
                entity.Property(e => e.TransactionDate).HasColumnName("fecha_transaccion").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Type).HasColumnName("tipo_transaccion");
                entity.Property(e => e.Description).HasColumnName("descripcion").HasMaxLength(255);
                entity.Property(e => e.Successful).HasColumnName("exitosa").HasDefaultValue(true);

                entity.HasOne(t => t.User)
                      .WithMany(u => u.Transactions)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Plan
            modelBuilder.Entity<Plan>(entity =>
            {
                entity.ToTable("planes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("nombre").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasColumnName("descripcion").HasMaxLength(255);
                entity.Property(e => e.MonthlyPrice).HasColumnName("precio_mensual").HasColumnType("decimal(10,2)");
                entity.Property(e => e.MaxPhoneNumbers).HasColumnName("numero_maximo_numeros");
                entity.Property(e => e.MessageCost).HasColumnName("costo_por_mensaje").HasColumnType("decimal(10,2)");
                entity.Property(e => e.WindowCost).HasColumnName("costo_por_ventana").HasColumnType("decimal(10,2)");
                entity.Property(e => e.WindowDuration).HasColumnName("duracion_ventana").HasDefaultValue(10);

                entity.HasMany(p => p.Subscriptions)
                      .WithOne(s => s.Plan)
                      .HasForeignKey(s => s.PlanId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Suscripción
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("suscripciones");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("usuario_id");
                entity.Property(e => e.PlanId).HasColumnName("plan_id");
                entity.Property(e => e.StripeSubscriptionId).HasColumnName("stripe_subscription_id").HasMaxLength(100);
                entity.Property(e => e.StartDate).HasColumnName("fecha_inicio").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.EndDate).HasColumnName("fecha_fin");
                entity.Property(e => e.Active).HasColumnName("activa").HasDefaultValue(true);
                entity.Property(e => e.PaymentStatus).HasColumnName("estado_pago").HasMaxLength(50);

                entity.HasOne(s => s.User)
                      .WithMany(u => u.Subscriptions)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de idiomas
            modelBuilder.Entity<LanguageConfiguration>(entity =>
            {
                entity.ToTable("configuracion_lenguaje");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Key).HasColumnName("clave").IsRequired().HasMaxLength(100);
                entity.Property(e => e.ValueEs).HasColumnName("valor_es").HasMaxLength(500);
                entity.Property(e => e.ValueEn).HasColumnName("valor_en").HasMaxLength(500);
                entity.Property(e => e.Description).HasColumnName("descripcion");

                entity.HasIndex(e => e.Key).IsUnique();
            });

            // Datos iniciales
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Datos iniciales para planes
            modelBuilder.Entity<Plan>().HasData(
                new Plan
                {
                    Id = Guid.Parse("2d8c9aaa-c22e-4e7d-b842-7ac19c3c5348"),
                    Name = "Básico",
                    Description = "Plan básico para emprendedores",
                    MonthlyPrice = 9.99m,
                    MaxPhoneNumbers = 1,
                    MessageCost = 0.02m,
                    WindowCost = 0.50m,
                    WindowDuration = 10
                },
                new Plan
                {
                    Id = Guid.Parse("c8b3c4b7-af63-442e-bd0e-31ef19a3c3b8"),
                    Name = "Profesional",
                    Description = "Plan para pequeños negocios",
                    MonthlyPrice = 19.99m,
                    MaxPhoneNumbers = 3,
                    MessageCost = 0.015m,
                    WindowCost = 0.40m,
                    WindowDuration = 15
                },
                new Plan
                {
                    Id = Guid.Parse("9a8d24bf-a732-44ad-8c2c-1c7f2b7d7e4d"),
                    Name = "Empresarial",
                    Description = "Plan para empresas establecidas",
                    MonthlyPrice = 49.99m,
                    MaxPhoneNumbers = 10,
                    MessageCost = 0.01m,
                    WindowCost = 0.30m,
                    WindowDuration = 20
                }
            );

            // Datos iniciales para idiomas
            SeedLanguageConfigurations(modelBuilder);
        }

        private void SeedLanguageConfigurations(ModelBuilder modelBuilder)
        {
            // Textos comunes
            modelBuilder.Entity<LanguageConfiguration>().HasData(
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.AppName",
                    ValueEs = "Números Empresariales",
                    ValueEn = "Business Numbers",
                    Description = "Nombre de la aplicación"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Welcome",
                    ValueEs = "Bienvenido",
                    ValueEn = "Welcome",
                    Description = "Mensaje de bienvenida"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Dashboard",
                    ValueEs = "Panel de Control",
                    ValueEn = "Dashboard",
                    Description = "Título del panel de control"
                }
            );

            // Menú principal
            modelBuilder.Entity<LanguageConfiguration>().HasData(
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Menu.Home",
                    ValueEs = "Inicio",
                    ValueEn = "Home",
                    Description = "Menú inicio"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Menu.PhoneNumbers",
                    ValueEs = "Mis Números",
                    ValueEn = "My Numbers",
                    Description = "Menú números"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Menu.MessageWindows",
                    ValueEs = "Ventanas de Mensajes",
                    ValueEn = "Message Windows",
                    Description = "Menú ventanas de mensajes"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Menu.Payments",
                    ValueEs = "Pagos",
                    ValueEn = "Payments",
                    Description = "Menú pagos"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Menu.Profile",
                    ValueEs = "Mi Perfil",
                    ValueEn = "My Profile",
                    Description = "Menú perfil"
                }
            );

            // Acciones comunes
            modelBuilder.Entity<LanguageConfiguration>().HasData(
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Actions.Save",
                    ValueEs = "Guardar",
                    ValueEn = "Save",
                    Description = "Botón guardar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Actions.Cancel",
                    ValueEs = "Cancelar",
                    ValueEn = "Cancel",
                    Description = "Botón cancelar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Actions.Edit",
                    ValueEs = "Editar",
                    ValueEn = "Edit",
                    Description = "Botón editar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Actions.Delete",
                    ValueEs = "Eliminar",
                    ValueEn = "Delete",
                    Description = "Botón eliminar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Actions.Add",
                    ValueEs = "Agregar",
                    ValueEn = "Add",
                    Description = "Botón agregar"
                }
            );
        }
    }
}