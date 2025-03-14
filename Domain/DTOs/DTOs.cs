using NumeroEmpresarial.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace NumeroEmpresarial.Domain.DTOs
{
    /// <summary>
    /// DTO para representar información resumida de un usuario
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public decimal Balance { get; set; }
        public string Language { get; set; }
        public bool Active { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime LastLogin { get; set; }
    }

    /// <summary>
    /// DTO para crear o actualizar un usuario
    /// </summary>
    public class UserCreateUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 6)]
        public string Password { get; set; }

        [StringLength(10)]
        public string Language { get; set; } = "es";
    }

    /// <summary>
    /// DTO para la autenticación de usuarios
    /// </summary>
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// DTO para respuesta de autenticación
    /// </summary>
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; }
    }

    /// <summary>
    /// DTO para representar un número de teléfono
    /// </summary>
    public class PhoneNumberDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Number { get; set; }
        public string PlivoId { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool Active { get; set; }
        public string RedirectionNumber { get; set; }
        public string Type { get; set; }
        public decimal MonthlyCost { get; set; }
        public UserDto User { get; set; }
    }

    /// <summary>
    /// DTO para la creación de un número de teléfono
    /// </summary>
    public class PhoneNumberCreateDto
    {
        [Required]
        [StringLength(20)]
        public string Number { get; set; }

        [Required]
        [StringLength(100)]
        public string PlivoId { get; set; }

        [Required]
        [StringLength(20)]
        public string RedirectionNumber { get; set; }

        public string Type { get; set; } = "Standard";

        [Required]
        public decimal MonthlyCost { get; set; }

        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    }

    /// <summary>
    /// DTO para actualizar el número de redirección
    /// </summary>
    public class RedirectionUpdateDto
    {
        [Required]
        [StringLength(20)]
        public string RedirectionNumber { get; set; }
    }

    /// <summary>
    /// DTO para representar una ventana de mensajes
    /// </summary>
    public class MessageWindowDto
    {
        public Guid Id { get; set; }
        public Guid PhoneNumberId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Active { get; set; }
        public int MaxMessages { get; set; }
        public int ReceivedMessages { get; set; }
        public decimal WindowCost { get; set; }
        public PhoneNumber PhoneNumber { get; set; }
        public IEnumerable<MessageDto> Messages { get; set; } = new List<MessageDto>();
    }

    /// <summary>
    /// DTO para la creación de una ventana de mensajes
    /// </summary>
    public class MessageWindowCreateDto
    {
        [Required]
        public Guid PhoneNumberId { get; set; }

        [Range(5, 120)]
        public int DurationMinutes { get; set; } = 10;

        [Range(1, 100)]
        public int MaxMessages { get; set; } = 10;

        public decimal Cost { get; set; } = 0.50m;
    }

    /// <summary>
    /// DTO para representar un mensaje
    /// </summary>
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid MessageWindowId { get; set; }
        public string From { get; set; }
        public string Text { get; set; }
        public DateTime ReceivedTime { get; set; }
        public bool Redirected { get; set; }
        public decimal MessageCost { get; set; }
    }

    /// <summary>
    /// DTO para representar una transacción
    /// </summary>
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string StripeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool Successful { get; set; }
    }

    /// <summary>
    /// DTO para la creación de una transacción
    /// </summary>
    public class TransactionCreateDto
    {
        [Required]
        public Guid UserId { get; set; }

        public string StripeId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Type { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public bool Successful { get; set; } = true;
    }

    /// <summary>
    /// DTO para representar un plan
    /// </summary>
    public class PlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public int MaxPhoneNumbers { get; set; }
        public decimal MessageCost { get; set; }
        public decimal WindowCost { get; set; }
        public int WindowDuration { get; set; }
    }

    /// <summary>
    /// DTO para representar una suscripción
    /// </summary>
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public string StripeSubscriptionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Active { get; set; }
        public string PaymentStatus { get; set; }
        public PlanDto Plan { get; set; }
    }

    /// <summary>
    /// DTO para representar un error de API
    /// </summary>
    public class ApiErrorDto
    {
        public string Error { get; set; }
        public int Code { get; set; }
        public object Details { get; set; }
    }

    /// <summary>
    /// DTO para solicitar la recarga de saldo
    /// </summary>
    public class RechargeBalanceDto
    {
        [Required]
        [Range(5, 10000)]
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// DTO para solicitar la suscripción a un plan
    /// </summary>
    public class SubscribeToPlanDto
    {
        [Required]
        public Guid PlanId { get; set; }
    }

    /// <summary>
    /// DTO para el webhook de Plivo de mensajes SMS
    /// </summary>
    public class PlivoSmsWebhookDto
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
        public string MessageUUID { get; set; }
    }
}