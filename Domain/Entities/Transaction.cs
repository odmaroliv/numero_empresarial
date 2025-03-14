using NumeroEmpresarial.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace NumeroEmpresarial.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        [MaxLength(100)]
        public string StripeId { get; set; }

        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public TransactionType Type { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public bool Successful { get; set; } = true;

        // Relaciones
        public virtual User User { get; set; }
    }
}