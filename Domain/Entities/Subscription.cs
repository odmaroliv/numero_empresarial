using System.ComponentModel.DataAnnotations;

namespace NumeroEmpresarial.Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid PlanId { get; set; }

        [MaxLength(100)]
        public string StripeSubscriptionId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime EndDate { get; set; }

        public bool Active { get; set; } = true;

        [MaxLength(50)]
        public string PaymentStatus { get; set; }

        // Relaciones
        public virtual User User { get; set; }
        public virtual Plan Plan { get; set; }
    }
}