using System.ComponentModel.DataAnnotations;

namespace NumeroEmpresarial.Domain.Entities
{
    public class Plan
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public decimal MonthlyPrice { get; set; }

        public int MaxPhoneNumbers { get; set; }

        public decimal MessageCost { get; set; }

        public decimal WindowCost { get; set; }

        public int WindowDuration { get; set; } = 10; // en minutos

        // Relaciones
        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}