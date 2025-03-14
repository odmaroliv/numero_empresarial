using System.ComponentModel.DataAnnotations;

namespace NumeroEmpresarial.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string ApiKey { get; set; } = Guid.NewGuid().ToString("N");

        public bool Active { get; set; } = true;

        public decimal Balance { get; set; } = 0;

        [MaxLength(10)]
        public string Language { get; set; } = "es";

        // Propiedades para gestión de refresh tokens
        public string RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Relaciones
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}