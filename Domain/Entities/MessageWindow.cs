namespace NumeroEmpresarial.Domain.Entities
{
    public class MessageWindow
    {
        public Guid Id { get; set; }

        public Guid PhoneNumberId { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public DateTime EndTime { get; set; }

        public bool Active { get; set; } = true;

        public int MaxMessages { get; set; } = 10;

        public int ReceivedMessages { get; set; } = 0;

        public decimal WindowCost { get; set; }

        // Relaciones
        public virtual PhoneNumber PhoneNumber { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}