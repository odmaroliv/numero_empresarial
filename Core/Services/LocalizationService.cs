using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Data;

namespace NumeroEmpresarial.Core.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<LocalizationService> _logger;
        private readonly string _cacheKey = "LocalizationData";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);

        public LocalizationService(
            ApplicationDbContext context,
            IMemoryCache memoryCache,
            ILogger<LocalizationService> logger)
        {
            _context = context;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<string> GetLocalizedTextAsync(string key, string language)
        {
            try
            {
                // Normalizar el idioma
                language = NormalizeLanguage(language);

                // Intentar obtener del caché
                if (!_memoryCache.TryGetValue(_cacheKey, out Dictionary<string, Dictionary<string, string>> cachedTranslations))
                {
                    // Si no está en caché, cargar de la base de datos
                    cachedTranslations = await LoadAllTranslationsAsync();

                    // Guardar en caché
                    _memoryCache.Set(_cacheKey, cachedTranslations, _cacheDuration);
                }

                // Buscar la traducción
                if (cachedTranslations.TryGetValue(key, out Dictionary<string, string> translations))
                {
                    if (translations.TryGetValue(language, out string translation))
                    {
                        return translation;
                    }

                    // Si no se encuentra en el idioma solicitado, intentar con el idioma predeterminado (español)
                    if (language != "es" && translations.TryGetValue("es", out string defaultTranslation))
                    {
                        _logger.LogWarning($"Traducción no encontrada para clave '{key}' en idioma '{language}', usando español");
                        return defaultTranslation;
                    }
                }

                // Si no se encuentra la clave, devolver la clave como respuesta
                _logger.LogWarning($"Clave de traducción no encontrada: '{key}'");
                return key;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener texto localizado para clave '{key}' e idioma '{language}'");
                return key; // En caso de error, devolver la clave
            }
        }

        public async Task<IDictionary<string, string>> GetAllTranslationsForLanguageAsync(string language)
        {
            try
            {
                // Normalizar el idioma
                language = NormalizeLanguage(language);

                // Intentar obtener del caché
                if (!_memoryCache.TryGetValue(_cacheKey, out Dictionary<string, Dictionary<string, string>> cachedTranslations))
                {
                    // Si no está en caché, cargar de la base de datos
                    cachedTranslations = await LoadAllTranslationsAsync();

                    // Guardar en caché
                    _memoryCache.Set(_cacheKey, cachedTranslations, _cacheDuration);
                }

                // Extraer todas las traducciones para el idioma solicitado
                var translations = new Dictionary<string, string>();

                foreach (var entry in cachedTranslations)
                {
                    if (entry.Value.TryGetValue(language, out string translation))
                    {
                        translations.Add(entry.Key, translation);
                    }
                    else if (language != "es" && entry.Value.TryGetValue("es", out string defaultTranslation))
                    {
                        // Si no hay traducción para el idioma solicitado, usar español
                        translations.Add(entry.Key, defaultTranslation);
                    }
                }

                return translations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener todas las traducciones para idioma '{language}'");
                return new Dictionary<string, string>();
            }
        }

        public async Task<bool> AddOrUpdateTranslationAsync(string key, string language, string value)
        {
            try
            {
                // Normalizar el idioma
                language = NormalizeLanguage(language);

                // Buscar si ya existe la entrada
                var translation = await _context.LanguageConfigurations
                    .FirstOrDefaultAsync(l => l.Key == key);

                if (translation == null)
                {
                    // Crear nueva entrada
                    translation = new Domain.Entities.LanguageConfiguration
                    {
                        Id = Guid.NewGuid(),
                        Key = key,
                        Description = $"Added programmatically on {DateTime.UtcNow}"
                    };

                    if (language == "es")
                    {
                        translation.ValueEs = value;
                    }
                    else if (language == "en")
                    {
                        translation.ValueEn = value;
                    }

                    _context.LanguageConfigurations.Add(translation);
                }
                else
                {
                    // Actualizar entrada existente
                    if (language == "es")
                    {
                        translation.ValueEs = value;
                    }
                    else if (language == "en")
                    {
                        translation.ValueEn = value;
                    }

                    _context.LanguageConfigurations.Update(translation);
                }

                await _context.SaveChangesAsync();

                // Invalidar caché
                _memoryCache.Remove(_cacheKey);

                _logger.LogInformation($"Traducción agregada/actualizada: clave='{key}', idioma='{language}'");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al agregar/actualizar traducción para clave '{key}' e idioma '{language}'");
                return false;
            }
        }

        public void ClearCache()
        {
            _memoryCache.Remove(_cacheKey);
            _logger.LogInformation("Caché de traducciones limpiado");
        }

        private async Task<Dictionary<string, Dictionary<string, string>>> LoadAllTranslationsAsync()
        {
            var result = new Dictionary<string, Dictionary<string, string>>();

            var translations = await _context.LanguageConfigurations.ToListAsync();

            foreach (var translation in translations)
            {
                var langValues = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(translation.ValueEs))
                {
                    langValues.Add("es", translation.ValueEs);
                }

                if (!string.IsNullOrEmpty(translation.ValueEn))
                {
                    langValues.Add("en", translation.ValueEn);
                }

                result.Add(translation.Key, langValues);
            }

            _logger.LogInformation($"Cargadas {translations.Count} traducciones de la base de datos");
            return result;
        }

        private string NormalizeLanguage(string language)
        {
            // Normalizar el idioma a un formato estándar (solo dos letras, minúsculas)
            if (string.IsNullOrEmpty(language))
            {
                return "es"; // Idioma predeterminado
            }

            // Tomar solo los dos primeros caracteres y convertir a minúsculas
            language = language.Substring(0, Math.Min(2, language.Length)).ToLowerInvariant();

            // Validar que sea un idioma soportado
            if (language != "es" && language != "en")
            {
                return "es"; // Si no es un idioma soportado, usar español
            }

            return language;
        }
    }
}