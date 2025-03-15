using NumeroEmpresarial.Domain.Entities;

namespace NumeroEmpresarial.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Asegurarse de que la base de datos esté creada
            context.Database.EnsureCreated();

            // Seed de planes si no existen
            if (!context.Plans.Any())
            {
                SeedPlans(context);
            }

            // Seed de configuraciones de idioma si no existen
            if (!context.LanguageConfigurations.Any())
            {
                SeedLanguageConfigurations(context);
            }
        }

        private static void SeedPlans(ApplicationDbContext context)
        {
            context.Plans.AddRange(
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

            context.SaveChanges();
        }

        private static void SeedLanguageConfigurations(ApplicationDbContext context)
        {
            // Textos comunes
            SeedCommonTranslations(context);

            // Traducciones para autenticación
            SeedAuthTranslations(context);

            // Traducciones para panel de control
            SeedDashboardTranslations(context);

            // Traducciones para números de teléfono
            SeedPhoneNumberTranslations(context);

            // Traducciones para ventanas de mensajes
            SeedMessageWindowTranslations(context);

            // Traducciones para planes
            SeedPlanTranslations(context);

            // Traducciones para pagos
            SeedPaymentTranslations(context);

            context.SaveChanges();
        }

        private static void SeedCommonTranslations(ApplicationDbContext context)
        {
            context.LanguageConfigurations.AddRange(
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
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Loading",
                    ValueEs = "Cargando...",
                    ValueEn = "Loading...",
                    Description = "Texto de carga"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Error",
                    ValueEs = "Error",
                    ValueEn = "Error",
                    Description = "Texto de error"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Success",
                    ValueEs = "Éxito",
                    ValueEn = "Success",
                    Description = "Texto de éxito"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Warning",
                    ValueEs = "Advertencia",
                    ValueEn = "Warning",
                    Description = "Texto de advertencia"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Info",
                    ValueEs = "Información",
                    ValueEn = "Information",
                    Description = "Texto de información"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Save",
                    ValueEs = "Guardar",
                    ValueEn = "Save",
                    Description = "Botón guardar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Cancel",
                    ValueEs = "Cancelar",
                    ValueEn = "Cancel",
                    Description = "Botón cancelar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Confirm",
                    ValueEs = "Confirmar",
                    ValueEn = "Confirm",
                    Description = "Botón confirmar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Edit",
                    ValueEs = "Editar",
                    ValueEn = "Edit",
                    Description = "Botón editar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Delete",
                    ValueEs = "Eliminar",
                    ValueEn = "Delete",
                    Description = "Botón eliminar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.View",
                    ValueEs = "Ver",
                    ValueEn = "View",
                    Description = "Botón ver"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Create",
                    ValueEs = "Crear",
                    ValueEn = "Create",
                    Description = "Botón crear"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Back",
                    ValueEs = "Volver",
                    ValueEn = "Back",
                    Description = "Botón volver"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Next",
                    ValueEs = "Siguiente",
                    ValueEn = "Next",
                    Description = "Botón siguiente"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Previous",
                    ValueEs = "Anterior",
                    ValueEn = "Previous",
                    Description = "Botón anterior"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Search",
                    ValueEs = "Buscar",
                    ValueEn = "Search",
                    Description = "Botón buscar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Filter",
                    ValueEs = "Filtrar",
                    ValueEn = "Filter",
                    Description = "Botón filtrar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Clear",
                    ValueEs = "Limpiar",
                    ValueEn = "Clear",
                    Description = "Botón limpiar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Apply",
                    ValueEs = "Aplicar",
                    ValueEn = "Apply",
                    Description = "Botón aplicar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Reset",
                    ValueEs = "Reiniciar",
                    ValueEn = "Reset",
                    Description = "Botón reiniciar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Close",
                    ValueEs = "Cerrar",
                    ValueEn = "Close",
                    Description = "Botón cerrar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Open",
                    ValueEs = "Abrir",
                    ValueEn = "Open",
                    Description = "Botón abrir"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Start",
                    ValueEs = "Iniciar",
                    ValueEn = "Start",
                    Description = "Botón iniciar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Stop",
                    ValueEs = "Detener",
                    ValueEn = "Stop",
                    Description = "Botón detener"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Pause",
                    ValueEs = "Pausar",
                    ValueEn = "Pause",
                    Description = "Botón pausar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Resume",
                    ValueEs = "Reanudar",
                    ValueEn = "Resume",
                    Description = "Botón reanudar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Submit",
                    ValueEs = "Enviar",
                    ValueEn = "Submit",
                    Description = "Botón enviar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Add",
                    ValueEs = "Agregar",
                    ValueEn = "Add",
                    Description = "Botón agregar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Remove",
                    ValueEs = "Quitar",
                    ValueEn = "Remove",
                    Description = "Botón quitar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Update",
                    ValueEs = "Actualizar",
                    ValueEn = "Update",
                    Description = "Botón actualizar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Refresh",
                    ValueEs = "Refrescar",
                    ValueEn = "Refresh",
                    Description = "Botón refrescar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Download",
                    ValueEs = "Descargar",
                    ValueEn = "Download",
                    Description = "Botón descargar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Upload",
                    ValueEs = "Subir",
                    ValueEn = "Upload",
                    Description = "Botón subir"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Import",
                    ValueEs = "Importar",
                    ValueEn = "Import",
                    Description = "Botón importar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Export",
                    ValueEs = "Exportar",
                    ValueEn = "Export",
                    Description = "Botón exportar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Print",
                    ValueEs = "Imprimir",
                    ValueEn = "Print",
                    Description = "Botón imprimir"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Copy",
                    ValueEs = "Copiar",
                    ValueEn = "Copy",
                    Description = "Botón copiar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Paste",
                    ValueEs = "Pegar",
                    ValueEn = "Paste",
                    Description = "Botón pegar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Cut",
                    ValueEs = "Cortar",
                    ValueEn = "Cut",
                    Description = "Botón cortar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.Select",
                    ValueEs = "Seleccionar",
                    ValueEn = "Select",
                    Description = "Botón seleccionar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.SelectAll",
                    ValueEs = "Seleccionar todo",
                    ValueEn = "Select all",
                    Description = "Botón seleccionar todo"
                }
            );
        }

        private static void SeedAuthTranslations(ApplicationDbContext context)
        {
            context.LanguageConfigurations.AddRange(
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.SignIn",
                    ValueEs = "Iniciar sesión",
                    ValueEn = "Sign in",
                    Description = "Título de inicio de sesión"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.SignOut",
                    ValueEs = "Cerrar sesión",
                    ValueEn = "Sign out",
                    Description = "Texto de cierre de sesión"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.Register",
                    ValueEs = "Registrarse",
                    ValueEn = "Register",
                    Description = "Título de registro"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.CreateAccount",
                    ValueEs = "Crear cuenta",
                    ValueEn = "Create account",
                    Description = "Título de creación de cuenta"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.RememberMe",
                    ValueEs = "Recordarme",
                    ValueEn = "Remember me",
                    Description = "Texto de recordar sesión"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.ForgotPassword",
                    ValueEs = "¿Olvidaste tu contraseña?",
                    ValueEn = "Forgot your password?",
                    Description = "Texto de olvido de contraseña"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.ResetPassword",
                    ValueEs = "Restablecer contraseña",
                    ValueEn = "Reset password",
                    Description = "Título de restablecimiento de contraseña"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.ConfirmPassword",
                    ValueEs = "Confirmar contraseña",
                    ValueEn = "Confirm password",
                    Description = "Texto de confirmación de contraseña"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.PasswordsMismatch",
                    ValueEs = "Las contraseñas no coinciden",
                    ValueEn = "Passwords do not match",
                    Description = "Error de contraseñas que no coinciden"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.CurrentPassword",
                    ValueEs = "Contraseña actual",
                    ValueEn = "Current password",
                    Description = "Texto de contraseña actual"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.NewPassword",
                    ValueEs = "Nueva contraseña",
                    ValueEn = "New password",
                    Description = "Texto de nueva contraseña"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.AcceptTerms",
                    ValueEs = "Acepto los",
                    ValueEn = "I accept the",
                    Description = "Texto de aceptación de términos"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.MustAcceptTerms",
                    ValueEs = "Debes aceptar los términos y condiciones",
                    ValueEn = "You must accept the terms and conditions",
                    Description = "Error de no aceptación de términos"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.DontHaveAccount",
                    ValueEs = "¿No tienes una cuenta?",
                    ValueEn = "Don't have an account?",
                    Description = "Texto de no tener cuenta"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.AlreadyHaveAccount",
                    ValueEs = "¿Ya tienes una cuenta?",
                    ValueEn = "Already have an account?",
                    Description = "Texto de ya tener cuenta"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.RegisterHere",
                    ValueEs = "Regístrate aquí",
                    ValueEn = "Register here",
                    Description = "Texto de registro aquí"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.SignInHere",
                    ValueEs = "Inicia sesión aquí",
                    ValueEn = "Sign in here",
                    Description = "Texto de inicio de sesión aquí"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.InvalidCredentials",
                    ValueEs = "Credenciales inválidas",
                    ValueEn = "Invalid credentials",
                    Description = "Error de credenciales inválidas"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.LoginFailed",
                    ValueEs = "Error al iniciar sesión",
                    ValueEn = "Login failed",
                    Description = "Error de inicio de sesión"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.EmailAlreadyRegistered",
                    ValueEs = "Este correo electrónico ya está registrado",
                    ValueEn = "This email is already registered",
                    Description = "Error de correo electrónico ya registrado"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Auth.PhoneHelp",
                    ValueEs = "Este número se usará para redireccionar los mensajes y llamadas",
                    ValueEn = "This number will be used to redirect messages and calls",
                    Description = "Ayuda de número de teléfono"
                }
            );
        }

        private static void SeedDashboardTranslations(ApplicationDbContext context)
        {
            context.LanguageConfigurations.AddRange(
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Dashboard.Title",
                    ValueEs = "Panel de Control",
                    ValueEn = "Dashboard",
                    Description = "Título del panel de control"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Dashboard.Balance",
                    ValueEs = "Saldo",
                    ValueEn = "Balance",
                    Description = "Texto de saldo"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Dashboard.ActiveNumbers",
                    ValueEs = "Números Activos",
                    ValueEn = "Active Numbers",
                    Description = "Texto de números activos"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Dashboard.ActiveWindows",
                    ValueEs = "Ventanas Activas",
                    ValueEn = "Active Windows",
                    Description = "Texto de ventanas activas"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Dashboard.TotalMessages",
                    ValueEs = "Mensajes Totales",
                    ValueEn = "Total Messages",
                    Description = "Texto de mensajes totales"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Dashboard.MyNumbers",
                    ValueEs = "Mis Números",
                    ValueEn = "My Numbers",
                    Description = "Texto de mis números"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Dashboard.Active",
                    ValueEs = "Activos",
                    ValueEn = "Active",
                    Description = "Texto de activos"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Dashboard.NewNumber",
                    ValueEs = "Nuevo Número",
                    ValueEn = "New Number",
                    Description = "Texto de nuevo número"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Dashboard.RecentTransactions",
                    ValueEs = "Transacciones Recientes",
                    ValueEn = "Recent Transactions",
                    Description = "Texto de transacciones recientes"
                }
            );
        }

        private static void SeedPhoneNumberTranslations(ApplicationDbContext context)
        {
            context.LanguageConfigurations.AddRange(
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.SearchTitle",
                    ValueEs = "Buscar Números",
                    ValueEn = "Search Numbers",
                    Description = "Título de búsqueda de números"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.SearchCriteria",
                    ValueEs = "Criterios de Búsqueda",
                    ValueEn = "Search Criteria",
                    Description = "Texto de criterios de búsqueda"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.Country",
                    ValueEs = "País",
                    ValueEn = "Country",
                    Description = "Texto de país"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.Type",
                    ValueEs = "Tipo",
                    ValueEn = "Type",
                    Description = "Texto de tipo"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.Pattern",
                    ValueEs = "Patrón",
                    ValueEn = "Pattern",
                    Description = "Texto de patrón"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.PatternHelp",
                    ValueEs = "Busque números que contengan secuencias específicas",
                    ValueEn = "Search for numbers that contain specific sequences",
                    Description = "Ayuda de patrón"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.SearchResults",
                    ValueEs = "Resultados de Búsqueda",
                    ValueEn = "Search Results",
                    Description = "Texto de resultados de búsqueda"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.NoResults",
                    ValueEs = "No se encontraron números que coincidan con tus criterios",
                    ValueEn = "No numbers found matching your criteria",
                    Description = "Texto de no hay resultados"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.SearchPrompt",
                    ValueEs = "Usa los filtros para buscar números disponibles",
                    ValueEn = "Use the filters to search for available numbers",
                    Description = "Texto de indicación de búsqueda"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.Number",
                    ValueEs = "Número",
                    ValueEn = "Number",
                    Description = "Texto de número"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.MonthlyCost",
                    ValueEs = "Costo Mensual",
                    ValueEn = "Monthly Cost",
                    Description = "Texto de costo mensual"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.Type.Local",
                    ValueEs = "Local",
                    ValueEn = "Local",
                    Description = "Texto de tipo local"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.Type.TollFree",
                    ValueEs = "Gratuito",
                    ValueEn = "Toll Free",
                    Description = "Texto de tipo gratuito"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.Type.Mobile",
                    ValueEs = "Móvil",
                    ValueEn = "Mobile",
                    Description = "Texto de tipo móvil"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.Rent",
                    ValueEs = "Alquilar",
                    ValueEn = "Rent",
                    Description = "Botón alquilar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.RentNumber",
                    ValueEs = "Alquilar Número",
                    ValueEn = "Rent Number",
                    Description = "Título de alquilar número"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.RentConfirm",
                    ValueEs = "¿Estás seguro de que deseas alquilar este número?",
                    ValueEn = "Are you sure you want to rent this number?",
                    Description = "Confirmación de alquiler"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.RedirectionNumber",
                    ValueEs = "Número de Redirección",
                    ValueEn = "Redirection Number",
                    Description = "Texto de número de redirección"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.RedirectionHelp",
                    ValueEs = "Los mensajes y llamadas se redirigirán a este número",
                    ValueEn = "Messages and calls will be redirected to this number",
                    Description = "Ayuda de número de redirección"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.RedirectionRequired",
                    ValueEs = "El número de redirección es obligatorio",
                    ValueEn = "Redirection number is required",
                    Description = "Error de número de redirección requerido"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.InsufficientBalance",
                    ValueEs = "Saldo insuficiente",
                    ValueEn = "Insufficient balance",
                    Description = "Error de saldo insuficiente"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.RentSuccess",
                    ValueEs = "Número alquilado con éxito",
                    ValueEn = "Number rented successfully",
                    Description = "Mensaje de éxito de alquiler"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.Redirection",
                    ValueEs = "Redirección",
                    ValueEn = "Redirection",
                    Description = "Texto de redirección"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.Windows",
                    ValueEs = "Ventanas",
                    ValueEn = "Windows",
                    Description = "Texto de ventanas"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.NoActive",
                    ValueEs = "No tienes números activos",
                    ValueEn = "You don't have active numbers",
                    Description = "Texto de no hay números activos"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "PhoneNumber.GetOneNow",
                    ValueEs = "Consigue uno ahora",
                    ValueEn = "Get one now",
                    Description = "Texto de conseguir uno ahora"
                }
            );
        }

        private static void SeedMessageWindowTranslations(ApplicationDbContext context)
        {
            context.LanguageConfigurations.AddRange(
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Title",
                    ValueEs = "Ventanas de Mensajes",
                    ValueEn = "Message Windows",
                    Description = "Título de ventanas de mensajes"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Create",
                    ValueEs = "Crear Ventana",
                    ValueEn = "Create Window",
                    Description = "Botón crear ventana"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.List",
                    ValueEs = "Lista de Ventanas",
                    ValueEn = "Window List",
                    Description = "Texto de lista de ventanas"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Created",
                    ValueEs = "Creada",
                    ValueEn = "Created",
                    Description = "Texto de creada"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.TimeWindow",
                    ValueEs = "Ventana de Tiempo",
                    ValueEn = "Time Window",
                    Description = "Texto de ventana de tiempo"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Messages",
                    ValueEs = "Mensajes",
                    ValueEn = "Messages",
                    Description = "Texto de mensajes"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.ViewDetails",
                    ValueEs = "Ver Detalles",
                    ValueEn = "View Details",
                    Description = "Texto de ver detalles"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Close",
                    ValueEs = "Cerrar",
                    ValueEn = "Close",
                    Description = "Botón cerrar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.NoWindows",
                    ValueEs = "No tienes ventanas de mensajes",
                    ValueEn = "You don't have message windows",
                    Description = "Texto de no hay ventanas"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Info",
                    ValueEs = "Información",
                    ValueEn = "Information",
                    Description = "Texto de información"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.HowItWorks",
                    ValueEs = "¿Cómo funciona?",
                    ValueEn = "How does it work?",
                    Description = "Texto de cómo funciona"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.HowItWorksDesc",
                    ValueEs = "Las ventanas de mensajes te permiten recibir y redireccionar mensajes durante un tiempo limitado",
                    ValueEn = "Message windows allow you to receive and redirect messages for a limited time",
                    Description = "Descripción de cómo funciona"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Feature1",
                    ValueEs = "Recibe mensajes durante un período de tiempo específico",
                    ValueEn = "Receive messages for a specific period of time",
                    Description = "Característica 1"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Feature2",
                    ValueEs = "Limita la cantidad de mensajes que puedes recibir",
                    ValueEn = "Limit the number of messages you can receive",
                    Description = "Característica 2"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Feature3",
                    ValueEs = "Redirecciona los mensajes a tu número personal",
                    ValueEn = "Redirect messages to your personal number",
                    Description = "Característica 3"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Feature4",
                    ValueEs = "Paga solo por los mensajes que recibes",
                    ValueEn = "Pay only for the messages you receive",
                    Description = "Característica 4"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Duration",
                    ValueEs = "Duración",
                    ValueEn = "Duration",
                    Description = "Texto de duración"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.DurationHelp",
                    ValueEs = "Duración en minutos (entre 5 y 120)",
                    ValueEn = "Duration in minutes (between 5 and 120)",
                    Description = "Ayuda de duración"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.MaxMessages",
                    ValueEs = "Mensajes Máximos",
                    ValueEn = "Max Messages",
                    Description = "Texto de mensajes máximos"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.MaxMessagesHelp",
                    ValueEs = "Cantidad máxima de mensajes que puedes recibir",
                    ValueEn = "Maximum number of messages you can receive",
                    Description = "Ayuda de mensajes máximos"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.DurationTooShort",
                    ValueEs = "La duración es demasiado corta",
                    ValueEn = "Duration is too short",
                    Description = "Error de duración demasiado corta"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.CloseConfirm",
                    ValueEs = "Confirmar Cierre",
                    ValueEn = "Confirm Close",
                    Description = "Título de confirmar cierre"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.CloseWarning",
                    ValueEs = "¿Estás seguro de que deseas cerrar esta ventana de mensajes?",
                    ValueEn = "Are you sure you want to close this message window?",
                    Description = "Advertencia de cierre"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.CloseWarningDetails",
                    ValueEs = "Una vez cerrada, no podrás recibir más mensajes en esta ventana",
                    ValueEn = "Once closed, you won't be able to receive more messages in this window",
                    Description = "Detalles de advertencia de cierre"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Closed",
                    ValueEs = "Cerrada",
                    ValueEn = "Closed",
                    Description = "Texto de cerrada"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.ViewMessages",
                    ValueEs = "Ver Mensajes",
                    ValueEn = "View Messages",
                    Description = "Texto de ver mensajes"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.NoActive",
                    ValueEs = "No tienes ventanas activas",
                    ValueEn = "You don't have active windows",
                    Description = "Texto de no hay ventanas activas"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.CreateOne",
                    ValueEs = "Crear una",
                    ValueEn = "Create one",
                    Description = "Texto de crear una"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.Details",
                    ValueEs = "Detalles",
                    ValueEn = "Details",
                    Description = "Texto de detalles"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "MessageWindow.NotFound",
                    ValueEs = "Ventana no encontrada",
                    ValueEn = "Window not found",
                    Description = "Error de ventana no encontrada"
                }
            );
        }

        private static void SeedPlanTranslations(ApplicationDbContext context)
        {
            context.LanguageConfigurations.AddRange(
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.Title",
                    ValueEs = "Planes",
                    ValueEn = "Plans",
                    Description = "Título de planes"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.Subtitle",
                    ValueEs = "Elige el plan que mejor se adapte a tus necesidades",
                    ValueEn = "Choose the plan that best suits your needs",
                    Description = "Subtítulo de planes"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.PerMonth",
                    ValueEs = "por mes",
                    ValueEn = "per month",
                    Description = "Texto de por mes"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.PhoneNumbers",
                    ValueEs = "números de teléfono",
                    ValueEn = "phone numbers",
                    Description = "Texto de números de teléfono"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.MessageCost",
                    ValueEs = "Costo por mensaje",
                    ValueEn = "Message cost",
                    Description = "Texto de costo por mensaje"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.WindowCost",
                    ValueEs = "Costo por ventana",
                    ValueEn = "Window cost",
                    Description = "Texto de costo por ventana"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.MinutesPerWindow",
                    ValueEs = "minutos por ventana",
                    ValueEn = "minutes per window",
                    Description = "Texto de minutos por ventana"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.FullSupport",
                    ValueEs = "Soporte completo",
                    ValueEn = "Full support",
                    Description = "Texto de soporte completo"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.CurrentPlan",
                    ValueEs = "Plan Actual",
                    ValueEn = "Current Plan",
                    Description = "Texto de plan actual"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.Subscribe",
                    ValueEs = "Suscribirse",
                    ValueEn = "Subscribe",
                    Description = "Botón suscribirse"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.CurrentSubscription",
                    ValueEs = "Suscripción Actual",
                    ValueEn = "Current Subscription",
                    Description = "Texto de suscripción actual"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.YouAreSubscribedTo",
                    ValueEs = "Estás suscrito al plan",
                    ValueEn = "You are subscribed to",
                    Description = "Texto de estás suscrito al plan"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.SubscriptionEndsOn",
                    ValueEs = "Tu suscripción finaliza el",
                    ValueEn = "Your subscription ends on",
                    Description = "Texto de suscripción finaliza"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.FAQ",
                    ValueEs = "Preguntas Frecuentes",
                    ValueEn = "Frequently Asked Questions",
                    Description = "Texto de preguntas frecuentes"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.FAQ1Title",
                    ValueEs = "¿Qué incluye mi suscripción?",
                    ValueEn = "What does my subscription include?",
                    Description = "Título de pregunta frecuente 1"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.FAQ1Text",
                    ValueEs = "Tu suscripción incluye acceso a números de teléfono, ventanas de mensajes y tarifas reducidas para mensajes y ventanas.",
                    ValueEn = "Your subscription includes access to phone numbers, message windows, and reduced rates for messages and windows.",
                    Description = "Texto de pregunta frecuente 1"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.FAQ2Title",
                    ValueEs = "¿Puedo cambiar de plan?",
                    ValueEn = "Can I change plans?",
                    Description = "Título de pregunta frecuente 2"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.FAQ2Text",
                    ValueEs = "Sí, puedes cambiar de plan en cualquier momento. El nuevo plan se activará al finalizar el período de facturación actual.",
                    ValueEn = "Yes, you can change plans at any time. The new plan will activate at the end of the current billing period.",
                    Description = "Texto de pregunta frecuente 2"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.FAQ3Title",
                    ValueEs = "¿Cómo cancelo mi suscripción?",
                    ValueEn = "How do I cancel my subscription?",
                    Description = "Título de pregunta frecuente 3"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.FAQ3Text",
                    ValueEs = "Puedes cancelar tu suscripción en cualquier momento desde la página de planes. Tu plan seguirá activo hasta el final del período de facturación.",
                    ValueEn = "You can cancel your subscription at any time from the plans page. Your plan will remain active until the end of the billing period.",
                    Description = "Texto de pregunta frecuente 3"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.CancelConfirm",
                    ValueEs = "Confirmar Cancelación",
                    ValueEn = "Confirm Cancellation",
                    Description = "Título de confirmar cancelación"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.CancelWarning",
                    ValueEs = "¿Estás seguro de que deseas cancelar tu suscripción?",
                    ValueEn = "Are you sure you want to cancel your subscription?",
                    Description = "Advertencia de cancelación"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.CancelWarningDetails",
                    ValueEs = "Tu plan seguirá activo hasta el final del período de facturación actual. Después, se te facturará según las tarifas estándar.",
                    ValueEn = "Your plan will remain active until the end of the current billing period. After that, you'll be billed at standard rates.",
                    Description = "Detalles de advertencia de cancelación"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.CancelSubscription",
                    ValueEs = "Cancelar Suscripción",
                    ValueEn = "Cancel Subscription",
                    Description = "Botón cancelar suscripción"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.NoActiveSubscription",
                    ValueEs = "No tienes una suscripción activa",
                    ValueEn = "You don't have an active subscription",
                    Description = "Error de no hay suscripción activa"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Plans.CancelSuccess",
                    ValueEs = "Suscripción cancelada con éxito",
                    ValueEn = "Subscription cancelled successfully",
                    Description = "Mensaje de éxito de cancelación"
                }
            );
        }

        private static void SeedPaymentTranslations(ApplicationDbContext context)
        {
            context.LanguageConfigurations.AddRange(
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.RechargeTitle",
                    ValueEs = "Recargar Saldo",
                    ValueEn = "Recharge Balance",
                    Description = "Título de recargar saldo"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.CurrentBalance",
                    ValueEs = "Saldo Actual",
                    ValueEn = "Current Balance",
                    Description = "Texto de saldo actual"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.BalanceDescription",
                    ValueEs = "El saldo se utiliza para alquilar números, crear ventanas de mensajes y pagar por los mensajes recibidos.",
                    ValueEn = "Balance is used to rent numbers, create message windows, and pay for received messages.",
                    Description = "Descripción de saldo"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.AmountToRecharge",
                    ValueEs = "Monto a Recargar",
                    ValueEn = "Amount to Recharge",
                    Description = "Texto de monto a recargar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.MinimumAmount",
                    ValueEs = "Monto mínimo:",
                    ValueEn = "Minimum amount:",
                    Description = "Texto de monto mínimo"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.ChooseAmount",
                    ValueEs = "Elige un monto",
                    ValueEn = "Choose an amount",
                    Description = "Texto de elegir monto"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.Basic",
                    ValueEs = "Básico",
                    ValueEn = "Basic",
                    Description = "Texto de básico"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.Standard",
                    ValueEs = "Estándar",
                    ValueEn = "Standard",
                    Description = "Texto de estándar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.Popular",
                    ValueEs = "Popular",
                    ValueEn = "Popular",
                    Description = "Texto de popular"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.Premium",
                    ValueEs = "Premium",
                    ValueEn = "Premium",
                    Description = "Texto de premium"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.Recharge",
                    ValueEs = "Recargar",
                    ValueEn = "Recharge",
                    Description = "Botón recargar"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.AmountTooSmall",
                    ValueEs = "El monto es demasiado pequeño",
                    ValueEn = "The amount is too small",
                    Description = "Error de monto demasiado pequeño"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.RechargeDescription",
                    ValueEs = "Recarga de saldo",
                    ValueEn = "Balance recharge",
                    Description = "Descripción de recarga"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Balance.RecentTransactions",
                    ValueEs = "Transacciones Recientes",
                    ValueEn = "Recent Transactions",
                    Description = "Texto de transacciones recientes"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Payment.CompletePayment",
                    ValueEs = "Completar Pago",
                    ValueEn = "Complete Payment",
                    Description = "Título de completar pago"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Payment.PreparingPayment",
                    ValueEs = "Preparando el pago...",
                    ValueEn = "Preparing payment...",
                    Description = "Texto de preparando pago"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Payment.SecurePayment",
                    ValueEs = "Pago Seguro",
                    ValueEn = "Secure Payment",
                    Description = "Texto de pago seguro"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Payment.SecurePaymentDesc",
                    ValueEs = "Los pagos son procesados de forma segura a través de Stripe.",
                    ValueEn = "Payments are securely processed through Stripe.",
                    Description = "Descripción de pago seguro"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Payment.ProceedToPayment",
                    ValueEs = "Proceder al Pago",
                    ValueEn = "Proceed to Payment",
                    Description = "Botón proceder al pago"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Payment.ProcessedByStripe",
                    ValueEs = "Procesado por Stripe",
                    ValueEn = "Processed by Stripe",
                    Description = "Texto de procesado por Stripe"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Transaction.Date",
                    ValueEs = "Fecha",
                    ValueEn = "Date",
                    Description = "Texto de fecha"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Transaction.Type",
                    ValueEs = "Tipo",
                    ValueEn = "Type",
                    Description = "Texto de tipo"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Transaction.Amount",
                    ValueEs = "Monto",
                    ValueEn = "Amount",
                    Description = "Texto de monto"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Transaction.NoRecent",
                    ValueEs = "No tienes transacciones recientes",
                    ValueEn = "You don't have recent transactions",
                    Description = "Texto de no hay transacciones recientes"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "TransactionType.Deposit",
                    ValueEs = "Depósito",
                    ValueEn = "Deposit",
                    Description = "Tipo de transacción depósito"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "TransactionType.PhoneNumberPurchase",
                    ValueEs = "Compra de Número",
                    ValueEn = "Phone Number Purchase",
                    Description = "Tipo de transacción compra de número"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "TransactionType.MessageWindowCreation",
                    ValueEs = "Creación de Ventana",
                    ValueEn = "Window Creation",
                    Description = "Tipo de transacción creación de ventana"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "TransactionType.MessageCharge",
                    ValueEs = "Cargo por Mensaje",
                    ValueEn = "Message Charge",
                    Description = "Tipo de transacción cargo por mensaje"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "TransactionType.Subscription",
                    ValueEs = "Suscripción",
                    ValueEn = "Subscription",
                    Description = "Tipo de transacción suscripción"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "TransactionType.Refund",
                    ValueEs = "Reembolso",
                    ValueEn = "Refund",
                    Description = "Tipo de transacción reembolso"
                },
                new LanguageConfiguration
                {
                    Id = Guid.NewGuid(),
                    Key = "Common.InsufficientBalance",
                    ValueEs = "Saldo insuficiente",
                    ValueEn = "Insufficient balance",
                    Description = "Error de saldo insuficiente"
                }
            );
        }
    }
}