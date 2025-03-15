using Microsoft.EntityFrameworkCore;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Data;
using NumeroEmpresarial.Domain.Entities;
using NumeroEmpresarial.Domain.Enums;
using Stripe;
using Stripe.Checkout;

namespace NumeroEmpresarial.Core.Services
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StripeService> _logger;
        private readonly string _webhookSecret;

        public StripeService(
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<StripeService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;

            // Configurar Stripe
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            _webhookSecret = _configuration["Stripe:WebhookSecret"];
        }

        public async Task<string> CreateCheckoutSessionAsync(Guid userId, decimal amount, string description)
        {
            try
            {
                _logger.LogInformation($"Creando sesión de checkout: usuario={userId}, monto=${amount}, descripción={description}");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("Usuario no encontrado");
                }

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(amount * 100), // Stripe trabaja en centavos
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Recarga de saldo",
                                    Description = description
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = _configuration["Stripe:SuccessUrl"] + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = _configuration["Stripe:CancelUrl"],
                    ClientReferenceId = userId.ToString()
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                _logger.LogInformation($"Sesión de checkout creada: {session.Id}");
                return session.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear sesión de checkout");
                throw;
            }
        }

        public async Task<bool> ProcessPaymentSuccessAsync(string sessionId)
        {
            try
            {
                _logger.LogInformation($"Procesando pago exitoso: sesión={sessionId}");

                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(sessionId);

                if (session == null || session.PaymentStatus != "paid")
                {
                    _logger.LogWarning($"Sesión no encontrada o pago no completado: {sessionId}");
                    return false;
                }

                var userId = Guid.Parse(session.ClientReferenceId);
                decimal amount = (decimal)session.AmountTotal / 100; // Convertir de centavos a dólares

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"Usuario no encontrado: {userId}");
                    return false;
                }

                // Crear transacción
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    StripeId = session.Id,
                    Amount = amount,
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.Deposit,
                    Description = "Recarga de saldo via Stripe",
                    Successful = true
                };

                // Actualizar saldo del usuario
                user.Balance += amount;

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Pago procesado con éxito: usuario={userId}, monto=${amount}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar pago: sesión={sessionId}");
                throw;
            }
        }

        public async Task<string> CreateSubscriptionCheckoutAsync(Guid userId, Guid planId)
        {
            try
            {
                _logger.LogInformation($"Creando sesión de suscripción: usuario={userId}, plan={planId}");

                var plan = await _context.Plans.FindAsync(planId);
                if (plan == null)
                {
                    throw new ArgumentException("Plan no encontrado");
                }

                // Crear o obtener el producto de Stripe
                var productOptions = new ProductCreateOptions
                {
                    Name = $"Plan {plan.Name}",
                    Description = plan.Description
                };

                var productService = new ProductService();
                var product = await productService.CreateAsync(productOptions);

                // Crear el precio de Stripe
                var priceOptions = new PriceCreateOptions
                {
                    UnitAmount = (long)(plan.MonthlyPrice * 100),
                    Currency = "usd",
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = "month"
                    },
                    Product = product.Id
                };

                var priceService = new PriceService();
                var price = await priceService.CreateAsync(priceOptions);

                // Crear la sesión de checkout
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = price.Id,
                            Quantity = 1
                        }
                    },
                    Mode = "subscription",
                    SuccessUrl = _configuration["Stripe:SubscriptionSuccessUrl"] + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = _configuration["Stripe:CancelUrl"],
                    ClientReferenceId = $"{userId}:{planId}" // Formato: userId:planId
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                _logger.LogInformation($"Sesión de suscripción creada: {session.Id}");
                return session.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear sesión de suscripción");
                throw;
            }
        }

        public async Task<bool> ProcessSubscriptionSuccessAsync(string sessionId)
        {
            try
            {
                _logger.LogInformation($"Procesando suscripción exitosa: sesión={sessionId}");

                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(sessionId);

                if (session == null || session.SubscriptionId == null)
                {
                    _logger.LogWarning($"Sesión no encontrada o suscripción no completada: {sessionId}");
                    return false;
                }

                // Extraer userId y planId del ClientReferenceId
                var parts = session.ClientReferenceId.Split(':');
                if (parts.Length != 2)
                {
                    _logger.LogWarning($"Formato de ClientReferenceId inválido: {session.ClientReferenceId}");
                    return false;
                }

                var userId = Guid.Parse(parts[0]);
                var planId = Guid.Parse(parts[1]);

                var user = await _context.Users.FindAsync(userId);
                var plan = await _context.Plans.FindAsync(planId);

                if (user == null || plan == null)
                {
                    _logger.LogWarning($"Usuario o plan no encontrado: usuario={userId}, plan={planId}");
                    return false;
                }

                // Cancelar suscripciones activas existentes
                var activeSubscriptions = await _context.Subscriptions
                    .Where(s => s.UserId == userId && s.Active)
                    .ToListAsync();

                foreach (var sub in activeSubscriptions)
                {
                    sub.Active = false;
                    sub.EndDate = DateTime.UtcNow;

                    // Cancelar en Stripe si es necesario
                    if (!string.IsNullOrEmpty(sub.StripeSubscriptionId))
                    {
                        var subscriptionService = new SubscriptionService();
                        await subscriptionService.CancelAsync(sub.StripeSubscriptionId, null);
                    }
                }

                // Crear nueva suscripción
                var subscription = new Domain.Entities.Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PlanId = planId,
                    StripeSubscriptionId = session.SubscriptionId,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    Active = true,
                    PaymentStatus = "active"
                };

                // Crear transacción
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    StripeId = session.Id,
                    Amount = plan.MonthlyPrice,
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.Subscription,
                    Description = $"Suscripción al plan {plan.Name}",
                    Successful = true
                };

                _context.Subscriptions.Add(subscription);
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Suscripción procesada con éxito: usuario={userId}, plan={planId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar suscripción: sesión={sessionId}");
                throw;
            }
        }

        public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
        {
            try
            {
                _logger.LogInformation($"Cancelando suscripción: {subscriptionId}");

                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.CancelAsync(subscriptionId, null);

                // Actualizar en la base de datos
                var dbSubscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscriptionId && s.Active);

                if (dbSubscription != null)
                {
                    dbSubscription.Active = false;
                    dbSubscription.EndDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Suscripción cancelada con éxito: {subscriptionId}");
                }
                else
                {
                    _logger.LogWarning($"Suscripción no encontrada en la base de datos: {subscriptionId}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cancelar suscripción: {subscriptionId}");
                return false;
            }
        }

        public async Task HandleWebhookAsync(string json, string signature)
        {
            try
            {
                _logger.LogInformation("Procesando webhook de Stripe");

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    _webhookSecret
                );

                // Manejar diferentes tipos de eventos usando nombres de eventos como cadenas
                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        var session = stripeEvent.Data.Object as Session;
                        if (session.Mode == "payment")
                        {
                            await ProcessPaymentSuccessAsync(session.Id);
                        }
                        else if (session.Mode == "subscription")
                        {
                            await ProcessSubscriptionSuccessAsync(session.Id);
                        }
                        break;

                    case "invoice.payment_succeeded":
                        var invoice = stripeEvent.Data.Object as Invoice;
                        // Actualizar estado de suscripción
                        await UpdateSubscriptionPaymentStatusAsync(invoice.SubscriptionId, "active");
                        break;

                    case "invoice.payment_failed":
                        var failedInvoice = stripeEvent.Data.Object as Invoice;
                        // Actualizar estado de suscripción
                        await UpdateSubscriptionPaymentStatusAsync(failedInvoice.SubscriptionId, "failed");
                        break;

                    case "customer.subscription.deleted":
                        var deletedSubscription = stripeEvent.Data.Object as Stripe.Subscription;
                        // Cancelar suscripción en la base de datos
                        await UpdateSubscriptionStatusAsync(deletedSubscription.Id, false);
                        break;
                }

                _logger.LogInformation($"Webhook procesado con éxito: tipo={stripeEvent.Type}");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error procesando webhook de Stripe");
                throw new ApplicationException($"Error procesando webhook: {ex.Message}", ex);
            }
        }

        private async Task UpdateSubscriptionPaymentStatusAsync(string stripeSubscriptionId, string status)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId && s.Active);

            if (subscription != null)
            {
                subscription.PaymentStatus = status;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Estado de pago de suscripción actualizado: {stripeSubscriptionId} -> {status}");
            }
            else
            {
                _logger.LogWarning($"Suscripción no encontrada para actualización de estado: {stripeSubscriptionId}");
            }
        }

        private async Task UpdateSubscriptionStatusAsync(string stripeSubscriptionId, bool active)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);

            if (subscription != null)
            {
                subscription.Active = active;
                if (!active)
                {
                    subscription.EndDate = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Estado de suscripción actualizado: {stripeSubscriptionId} -> {(active ? "activa" : "inactiva")}");
            }
            else
            {
                _logger.LogWarning($"Suscripción no encontrada para actualización: {stripeSubscriptionId}");
            }
        }
    }
}