using System.ComponentModel.DataAnnotations;

namespace NumeroEmpresarial.Domain.Entities
{
    public class LanguageConfiguration
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; }

        [MaxLength(500)]
        public string ValueEs { get; set; }

        [MaxLength(500)]
        public string ValueEn { get; set; }

        public string Description { get; set; }
    }
}