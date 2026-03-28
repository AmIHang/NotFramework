namespace Not.Core.Localization
{
    /// <summary>
    /// Defines the languages supported by the application.
    /// LocalizedString properties on entities will have one value slot per supported culture.
    /// Set LocalizationConfig.Current at application startup before any entity access.
    /// </summary>
    public class LocalizationConfig
    {
        public IReadOnlyList<Culture> SupportedCultures { get; }

        public LocalizationConfig(IEnumerable<Culture> supportedCultures)
        {
            SupportedCultures = supportedCultures.ToList().AsReadOnly();
        }

        /// <summary>
        /// The active configuration. Defaults to Invariant + German.
        /// Override at application startup to match your supported languages.
        /// </summary>
        public static LocalizationConfig Current { get; set; } = new LocalizationConfig(
            [CultureService.Invariant, CultureService.German]
        );
    }
}
