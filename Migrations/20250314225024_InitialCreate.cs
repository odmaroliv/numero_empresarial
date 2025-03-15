using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NumeroEmpresarial.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuracion_lenguaje",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clave = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valor_es = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    valor_en = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_lenguaje", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "planes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    precio_mensual = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    numero_maximo_numeros = table.Column<int>(type: "integer", nullable: false),
                    costo_por_mensaje = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    costo_por_ventana = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    duracion_ventana = table.Column<int>(type: "integer", nullable: false, defaultValue: 10)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ultimo_acceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    api_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    saldo = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    idioma = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "es"),
                    refresh_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    refresh_token_expiry_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "numeros_telefono",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    plivo_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_adquisicion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_expiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    numero_redireccion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tipo_numero = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    costo_por_mes = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_numeros_telefono", x => x.id);
                    table.ForeignKey(
                        name: "FK_numeros_telefono_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "suscripciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stripe_subscription_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    estado_pago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suscripciones", x => x.id);
                    table.ForeignKey(
                        name: "FK_suscripciones_planes_plan_id",
                        column: x => x.plan_id,
                        principalTable: "planes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_suscripciones_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transacciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stripe_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    fecha_transaccion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    tipo_transaccion = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    exitosa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transacciones", x => x.id);
                    table.ForeignKey(
                        name: "FK_transacciones_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ventanas_mensaje",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_telefono_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    numero_maximo_mensajes = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    mensajes_recibidos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    costo_por_ventana = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventanas_mensaje", x => x.id);
                    table.ForeignKey(
                        name: "FK_ventanas_mensaje_numeros_telefono_numero_telefono_id",
                        column: x => x.numero_telefono_id,
                        principalTable: "numeros_telefono",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mensajes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ventana_mensaje_id = table.Column<Guid>(type: "uuid", nullable: false),
                    de = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    fecha_recepcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    redirigido = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    costo_mensaje = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensajes", x => x.id);
                    table.ForeignKey(
                        name: "FK_mensajes_ventanas_mensaje_ventana_mensaje_id",
                        column: x => x.ventana_mensaje_id,
                        principalTable: "ventanas_mensaje",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "configuracion_lenguaje",
                columns: new[] { "id", "descripcion", "clave", "valor_en", "valor_es" },
                values: new object[,]
                {
                    { new Guid("08e18373-fbf7-4109-a0df-f3e9f3e79c46"), "Botón editar", "Actions.Edit", "Edit", "Editar" },
                    { new Guid("103aaa6c-cae3-499e-af88-41efe7b14111"), "Menú números", "Menu.PhoneNumbers", "My Numbers", "Mis Números" },
                    { new Guid("17e970f2-1b0d-4b0a-81ec-3d1509718e0d"), "Botón eliminar", "Actions.Delete", "Delete", "Eliminar" },
                    { new Guid("52e88d58-057e-482d-92bd-f46d3875aa1e"), "Botón cancelar", "Actions.Cancel", "Cancel", "Cancelar" },
                    { new Guid("75f09371-b04f-488f-9782-1402c9991368"), "Botón agregar", "Actions.Add", "Add", "Agregar" },
                    { new Guid("7bd7d788-cd4a-4111-a7d4-da111c133f1e"), "Menú inicio", "Menu.Home", "Home", "Inicio" },
                    { new Guid("b8894420-9821-4ef9-91ec-4dacda87b7f7"), "Menú pagos", "Menu.Payments", "Payments", "Pagos" },
                    { new Guid("cd596103-2311-4935-a23a-251cd99d11d7"), "Menú perfil", "Menu.Profile", "My Profile", "Mi Perfil" },
                    { new Guid("d6d4735d-0ebc-4bfe-bded-90eb7d1d8658"), "Título del panel de control", "Common.Dashboard", "Dashboard", "Panel de Control" },
                    { new Guid("ddfd397d-8e09-47c6-a3cf-48938de9a390"), "Nombre de la aplicación", "Common.AppName", "Business Numbers", "Números Empresariales" },
                    { new Guid("e3a7593a-4309-449e-9b71-b862bcd770ea"), "Mensaje de bienvenida", "Common.Welcome", "Welcome", "Bienvenido" },
                    { new Guid("f39898e1-2aa6-4400-87fe-fcc355dba0d8"), "Botón guardar", "Actions.Save", "Save", "Guardar" },
                    { new Guid("f5ff4943-67bb-497b-8b16-78c3d8dbd0d7"), "Menú ventanas de mensajes", "Menu.MessageWindows", "Message Windows", "Ventanas de Mensajes" }
                });

            migrationBuilder.InsertData(
                table: "planes",
                columns: new[] { "id", "descripcion", "numero_maximo_numeros", "costo_por_mensaje", "precio_mensual", "nombre", "costo_por_ventana", "duracion_ventana" },
                values: new object[,]
                {
                    { new Guid("2d8c9aaa-c22e-4e7d-b842-7ac19c3c5348"), "Plan básico para emprendedores", 1, 0.02m, 9.99m, "Básico", 0.50m, 10 },
                    { new Guid("9a8d24bf-a732-44ad-8c2c-1c7f2b7d7e4d"), "Plan para empresas establecidas", 10, 0.01m, 49.99m, "Empresarial", 0.30m, 20 },
                    { new Guid("c8b3c4b7-af63-442e-bd0e-31ef19a3c3b8"), "Plan para pequeños negocios", 3, 0.015m, 19.99m, "Profesional", 0.40m, 15 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_configuracion_lenguaje_clave",
                table: "configuracion_lenguaje",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mensajes_ventana_mensaje_id",
                table: "mensajes",
                column: "ventana_mensaje_id");

            migrationBuilder.CreateIndex(
                name: "IX_numeros_telefono_numero",
                table: "numeros_telefono",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_numeros_telefono_usuario_id",
                table: "numeros_telefono",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_suscripciones_plan_id",
                table: "suscripciones",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_suscripciones_usuario_id",
                table: "suscripciones",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_transacciones_usuario_id",
                table: "transacciones",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ventanas_mensaje_numero_telefono_id",
                table: "ventanas_mensaje",
                column: "numero_telefono_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion_lenguaje");

            migrationBuilder.DropTable(
                name: "mensajes");

            migrationBuilder.DropTable(
                name: "suscripciones");

            migrationBuilder.DropTable(
                name: "transacciones");

            migrationBuilder.DropTable(
                name: "ventanas_mensaje");

            migrationBuilder.DropTable(
                name: "planes");

            migrationBuilder.DropTable(
                name: "numeros_telefono");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
