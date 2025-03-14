namespace NumeroEmpresarial.Core.Interfaces
{
    public interface IPlivoService
    {
        /// <summary>
        /// Busca números de teléfono disponibles según los criterios especificados
        /// </summary>
        /// <param name="countryIso">Código ISO del país (ej. "US", "ES")</param>
        /// <param name="type">Tipo de número (opcional): "local", "tollfree", "mobile"</param>
        /// <param name="pattern">Patrón para buscar (opcional): ej. "415", "800"</param>
        /// <returns>Lista de números de teléfono disponibles</returns>
        Task<List<PhoneNumberResource>> SearchAvailableNumbersAsync(string countryIso, string type = null, string pattern = null);

        /// <summary>
        /// Alquila un número de teléfono 
        /// </summary>
        /// <param name="phoneNumber">Número a alquilar (con formato internacional)</param>
        /// <returns>ID del número alquilado en Plivo</returns>
        Task<string> RentPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// Libera un número de teléfono alquilado
        /// </summary>
        /// <param name="plivoPhoneNumberId">ID del número en Plivo</param>
        /// <returns>True si se liberó correctamente</returns>
        Task<bool> ReleasePhoneNumberAsync(string plivoPhoneNumberId);

        /// <summary>
        /// Envía un mensaje SMS
        /// </summary>
        /// <param name="from">Número remitente</param>
        /// <param name="to">Número destinatario</param>
        /// <param name="text">Contenido del mensaje</param>
        /// <returns>Respuesta de la API de mensajes de Plivo</returns>
        Task<MessageResponse> SendSmsAsync(string from, string to, string text);

        /// <summary>
        /// Configura la URL de webhook para redirección de mensajes
        /// </summary>
        /// <param name="plivoPhoneNumberId">ID del número en Plivo</param>
        /// <param name="webhookUrl">URL del webhook</param>
        /// <returns>True si se configuró correctamente</returns>
        Task<bool> SetupRedirectionWebhookAsync(string plivoPhoneNumberId, string webhookUrl);

        /// <summary>
        /// Obtiene todos los números asociados a la cuenta de Plivo
        /// </summary>
        /// <returns>Lista de números de la cuenta</returns>
        Task<List<PhoneNumberResource>> GetAccountNumbersAsync();

        /// <summary>
        /// Obtiene el costo mensual de alquiler de un número
        /// </summary>
        /// <param name="phoneNumber">Número a consultar</param>
        /// <returns>Costo mensual del número</returns>
        Task<decimal> GetNumberRentalCostAsync(string phoneNumber);
    }
}