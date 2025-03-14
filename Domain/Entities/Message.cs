using System.ComponentModel.DataAnnotations;

namespace NumeroEmpresarial.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; set; }

        public Guid MessageWindowId { get; set; }

        [Required]
        [MaxLength(20)]
        public string From { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime ReceivedTime { get; set; } = DateTime.UtcNow;

        public bool Redirected { get; set; } = false;

        public decimal MessageCost { get; set; }

        // Relaciones
        public virtual MessageWindow MessageWindow { get; set; }
    }
}