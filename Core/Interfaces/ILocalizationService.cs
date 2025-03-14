namespace NumeroEmpresarial.Core.Interfaces
{
    public interface ILocalizationService
    {
        /// <summary>
        /// Obtiene un texto traducido según la clave e idioma especificados
        /// </summary>
        /// <param name="key">Clave de traducción</param>
        /// <param name="language">Código de idioma (es, en)</param>
        /// <returns>Texto traducido o la clave si no se encuentra</returns>
        Task<string> GetLocalizedTextAsync(string key, string language);

        /// <summary>
        /// Obtiene todas las traducciones disponibles para un idioma específico
        /// </summary>
        /// <param name="language">Código de idioma (es, en)</param>
        /// <returns>Diccionario con clave de traducción y texto traducido</returns>
        Task<IDictionary<string, string>> GetAllTranslationsForLanguageAsync(string language);

        /// <summary>
        /// Agrega o actualiza una traducción
        /// </summary>
        /// <param name="key">Clave de traducción</param>
        /// <param name="language">Código de idioma (es, en)</param>
        /// <param name="value">Texto traducido</param>
        /// <returns>True si se agregó/actualizó correctamente</returns>
        Task<bool> AddOrUpdateTranslationAsync(string key, string language, string value);

        /// <summary>
        /// Limpia la caché de traducciones
        /// </summary>
        void ClearCache();
    }
}