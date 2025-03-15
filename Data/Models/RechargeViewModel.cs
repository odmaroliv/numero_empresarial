using System.ComponentModel.DataAnnotations;

public class RechargeViewModel
{
    [Required]
    [Range(5, 1000, ErrorMessage = "El monto debe estar entre 5 y 1000")]
    public decimal Amount { get; set; } = 20;

    public decimal CurrentBalance { get; set; }
}