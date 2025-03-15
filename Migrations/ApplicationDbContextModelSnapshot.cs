﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NumeroEmpresarial.Data;

#nullable disable

namespace NumeroEmpresarial.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.LanguageConfiguration", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("descripcion");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("clave");

                    b.Property<string>("ValueEn")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("valor_en");

                    b.Property<string>("ValueEs")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("valor_es");

                    b.HasKey("Id");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.ToTable("configuracion_lenguaje", (string)null);

                    b.HasData(
                        new
                        {
                            Id = new Guid("ddfd397d-8e09-47c6-a3cf-48938de9a390"),
                            Description = "Nombre de la aplicación",
                            Key = "Common.AppName",
                            ValueEn = "Business Numbers",
                            ValueEs = "Números Empresariales"
                        },
                        new
                        {
                            Id = new Guid("e3a7593a-4309-449e-9b71-b862bcd770ea"),
                            Description = "Mensaje de bienvenida",
                            Key = "Common.Welcome",
                            ValueEn = "Welcome",
                            ValueEs = "Bienvenido"
                        },
                        new
                        {
                            Id = new Guid("d6d4735d-0ebc-4bfe-bded-90eb7d1d8658"),
                            Description = "Título del panel de control",
                            Key = "Common.Dashboard",
                            ValueEn = "Dashboard",
                            ValueEs = "Panel de Control"
                        },
                        new
                        {
                            Id = new Guid("7bd7d788-cd4a-4111-a7d4-da111c133f1e"),
                            Description = "Menú inicio",
                            Key = "Menu.Home",
                            ValueEn = "Home",
                            ValueEs = "Inicio"
                        },
                        new
                        {
                            Id = new Guid("103aaa6c-cae3-499e-af88-41efe7b14111"),
                            Description = "Menú números",
                            Key = "Menu.PhoneNumbers",
                            ValueEn = "My Numbers",
                            ValueEs = "Mis Números"
                        },
                        new
                        {
                            Id = new Guid("f5ff4943-67bb-497b-8b16-78c3d8dbd0d7"),
                            Description = "Menú ventanas de mensajes",
                            Key = "Menu.MessageWindows",
                            ValueEn = "Message Windows",
                            ValueEs = "Ventanas de Mensajes"
                        },
                        new
                        {
                            Id = new Guid("b8894420-9821-4ef9-91ec-4dacda87b7f7"),
                            Description = "Menú pagos",
                            Key = "Menu.Payments",
                            ValueEn = "Payments",
                            ValueEs = "Pagos"
                        },
                        new
                        {
                            Id = new Guid("cd596103-2311-4935-a23a-251cd99d11d7"),
                            Description = "Menú perfil",
                            Key = "Menu.Profile",
                            ValueEn = "My Profile",
                            ValueEs = "Mi Perfil"
                        },
                        new
                        {
                            Id = new Guid("f39898e1-2aa6-4400-87fe-fcc355dba0d8"),
                            Description = "Botón guardar",
                            Key = "Actions.Save",
                            ValueEn = "Save",
                            ValueEs = "Guardar"
                        },
                        new
                        {
                            Id = new Guid("52e88d58-057e-482d-92bd-f46d3875aa1e"),
                            Description = "Botón cancelar",
                            Key = "Actions.Cancel",
                            ValueEn = "Cancel",
                            ValueEs = "Cancelar"
                        },
                        new
                        {
                            Id = new Guid("08e18373-fbf7-4109-a0df-f3e9f3e79c46"),
                            Description = "Botón editar",
                            Key = "Actions.Edit",
                            ValueEn = "Edit",
                            ValueEs = "Editar"
                        },
                        new
                        {
                            Id = new Guid("17e970f2-1b0d-4b0a-81ec-3d1509718e0d"),
                            Description = "Botón eliminar",
                            Key = "Actions.Delete",
                            ValueEn = "Delete",
                            ValueEs = "Eliminar"
                        },
                        new
                        {
                            Id = new Guid("75f09371-b04f-488f-9782-1402c9991368"),
                            Description = "Botón agregar",
                            Key = "Actions.Add",
                            ValueEn = "Add",
                            ValueEs = "Agregar"
                        });
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("From")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("de");

                    b.Property<decimal>("MessageCost")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("costo_mensaje");

                    b.Property<Guid>("MessageWindowId")
                        .HasColumnType("uuid")
                        .HasColumnName("ventana_mensaje_id");

                    b.Property<DateTime>("ReceivedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("fecha_recepcion")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<bool>("Redirected")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("redirigido");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("texto");

                    b.HasKey("Id");

                    b.HasIndex("MessageWindowId");

                    b.ToTable("mensajes", (string)null);
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.MessageWindow", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool>("Active")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true)
                        .HasColumnName("activa");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("fecha_fin");

                    b.Property<int>("MaxMessages")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(10)
                        .HasColumnName("numero_maximo_mensajes");

                    b.Property<Guid>("PhoneNumberId")
                        .HasColumnType("uuid")
                        .HasColumnName("numero_telefono_id");

                    b.Property<int>("ReceivedMessages")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0)
                        .HasColumnName("mensajes_recibidos");

                    b.Property<DateTime>("StartTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("fecha_inicio")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<decimal>("WindowCost")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("costo_por_ventana");

                    b.HasKey("Id");

                    b.HasIndex("PhoneNumberId");

                    b.ToTable("ventanas_mensaje", (string)null);
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.PhoneNumber", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("AcquisitionDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("fecha_adquisicion")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<bool>("Active")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true)
                        .HasColumnName("activo");

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("fecha_expiracion");

                    b.Property<decimal>("MonthlyCost")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("costo_por_mes");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("numero");

                    b.Property<string>("PlivoId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("plivo_id");

                    b.Property<string>("RedirectionNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("numero_redireccion");

                    b.Property<int>("Type")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0)
                        .HasColumnName("tipo_numero");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("usuario_id");

                    b.HasKey("Id");

                    b.HasIndex("Number")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("numeros_telefono", (string)null);
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.Plan", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("descripcion");

                    b.Property<int>("MaxPhoneNumbers")
                        .HasColumnType("integer")
                        .HasColumnName("numero_maximo_numeros");

                    b.Property<decimal>("MessageCost")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("costo_por_mensaje");

                    b.Property<decimal>("MonthlyPrice")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("precio_mensual");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("nombre");

                    b.Property<decimal>("WindowCost")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("costo_por_ventana");

                    b.Property<int>("WindowDuration")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(10)
                        .HasColumnName("duracion_ventana");

                    b.HasKey("Id");

                    b.ToTable("planes", (string)null);

                    b.HasData(
                        new
                        {
                            Id = new Guid("2d8c9aaa-c22e-4e7d-b842-7ac19c3c5348"),
                            Description = "Plan básico para emprendedores",
                            MaxPhoneNumbers = 1,
                            MessageCost = 0.02m,
                            MonthlyPrice = 9.99m,
                            Name = "Básico",
                            WindowCost = 0.50m,
                            WindowDuration = 10
                        },
                        new
                        {
                            Id = new Guid("c8b3c4b7-af63-442e-bd0e-31ef19a3c3b8"),
                            Description = "Plan para pequeños negocios",
                            MaxPhoneNumbers = 3,
                            MessageCost = 0.015m,
                            MonthlyPrice = 19.99m,
                            Name = "Profesional",
                            WindowCost = 0.40m,
                            WindowDuration = 15
                        },
                        new
                        {
                            Id = new Guid("9a8d24bf-a732-44ad-8c2c-1c7f2b7d7e4d"),
                            Description = "Plan para empresas establecidas",
                            MaxPhoneNumbers = 10,
                            MessageCost = 0.01m,
                            MonthlyPrice = 49.99m,
                            Name = "Empresarial",
                            WindowCost = 0.30m,
                            WindowDuration = 20
                        });
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.Subscription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool>("Active")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true)
                        .HasColumnName("activa");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("fecha_fin");

                    b.Property<string>("PaymentStatus")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("estado_pago");

                    b.Property<Guid>("PlanId")
                        .HasColumnType("uuid")
                        .HasColumnName("plan_id");

                    b.Property<DateTime>("StartDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("fecha_inicio")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("StripeSubscriptionId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("stripe_subscription_id");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("usuario_id");

                    b.HasKey("Id");

                    b.HasIndex("PlanId");

                    b.HasIndex("UserId");

                    b.ToTable("suscripciones", (string)null);
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.Transaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("monto");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("descripcion");

                    b.Property<string>("StripeId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("stripe_id");

                    b.Property<bool>("Successful")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true)
                        .HasColumnName("exitosa");

                    b.Property<DateTime>("TransactionDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("fecha_transaccion")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("tipo_transaccion");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("usuario_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("transacciones", (string)null);
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool>("Active")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true)
                        .HasColumnName("activo");

                    b.Property<string>("ApiKey")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("api_key");

                    b.Property<decimal>("Balance")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(10,2)")
                        .HasDefaultValue(0m)
                        .HasColumnName("saldo");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("email");

                    b.Property<string>("Language")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasDefaultValue("es")
                        .HasColumnName("idioma");

                    b.Property<DateTime>("LastLogin")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("ultimo_acceso")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("nombre");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password_hash");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("telefono");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("refresh_token");

                    b.Property<DateTime?>("RefreshTokenExpiryTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("refresh_token_expiry_time");

                    b.Property<DateTime>("RegistrationDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("fecha_registro")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("usuarios", (string)null);
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.Message", b =>
                {
                    b.HasOne("NumeroEmpresarial.Domain.Entities.MessageWindow", "MessageWindow")
                        .WithMany("Messages")
                        .HasForeignKey("MessageWindowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MessageWindow");
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.MessageWindow", b =>
                {
                    b.HasOne("NumeroEmpresarial.Domain.Entities.PhoneNumber", "PhoneNumber")
                        .WithMany("MessageWindows")
                        .HasForeignKey("PhoneNumberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PhoneNumber");
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.PhoneNumber", b =>
                {
                    b.HasOne("NumeroEmpresarial.Domain.Entities.User", "User")
                        .WithMany("PhoneNumbers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.Subscription", b =>
                {
                    b.HasOne("NumeroEmpresarial.Domain.Entities.Plan", "Plan")
                        .WithMany("Subscriptions")
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NumeroEmpresarial.Domain.Entities.User", "User")
                        .WithMany("Subscriptions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plan");

                    b.Navigation("User");
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.Transaction", b =>
                {
                    b.HasOne("NumeroEmpresarial.Domain.Entities.User", "User")
                        .WithMany("Transactions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.MessageWindow", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.PhoneNumber", b =>
                {
                    b.Navigation("MessageWindows");
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.Plan", b =>
                {
                    b.Navigation("Subscriptions");
                });

            modelBuilder.Entity("NumeroEmpresarial.Domain.Entities.User", b =>
                {
                    b.Navigation("PhoneNumbers");

                    b.Navigation("Subscriptions");

                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
