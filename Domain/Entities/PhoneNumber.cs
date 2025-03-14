using NumeroEmpresarial.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace NumeroEmpresarial.Domain.Entities
{
    public class PhoneNumber
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Number { get; set; }

        [Required]
        [MaxLength(100)]
        public string PlivoId { get; set; }

        public DateTime AcquisitionDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpirationDate { get; set; }

        public bool Active { get; set; } = true;

        [Required]
        [MaxLength(20)]
        public string RedirectionNumber { get; set; }

        public PhoneNumberType Type { get; set; } = PhoneNumberType.Standard;

        public decimal MonthlyCost { get; set; }

        // Relaciones
        public virtual User User { get; set; }
        public virtual ICollection<MessageWindow> MessageWindows { get; set; }
    }
}