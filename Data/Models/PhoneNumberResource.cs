namespace NumeroEmpresarial.Domain.Models
{
    /// <summary>
    /// Representa un número de teléfono disponible en Plivo
    /// </summary>
    public class PhoneNumberResource
    {
        /// <summary>
        /// Número de teléfono completo con código de país
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Tipo de número (local, tollfree, mobile)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// País del número
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Costo mensual del alquiler
        /// </summary>
        public string MonthlyRentalRate { get; set; }

        /// <summary>
        /// Costo de configuración inicial
        /// </summary>
        public string SetupRate { get; set; }

        /// <summary>
        /// ID del número en Plivo (una vez alquilado)
        /// </summary>
        public string Id { get; set; }
    }

    /// <summary>
    /// Respuesta de un mensaje enviado por Plivo
    /// </summary>
    public class MessageResponse
    {
        /// <summary>
        /// Estado del mensaje
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// ID único del mensaje
        /// </summary>
        public List<string> MessageUuid { get; set; }

        /// <summary>
        /// Mensaje de error (si lo hay)
        /// </summary>
        public string Error { get; set; }
    }
}