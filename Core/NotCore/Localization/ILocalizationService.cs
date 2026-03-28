using Not.Core.Service;

namespace Not.Core.Localization
{
    /// <summary>
    /// Injectable service for accessing the active localization configuration.
    /// Register as a singleton. Configure supported languages via LocalizationConfig.Current
    /// at application startup before the DbContext is first created.
    /// </summary>
    public interface ILocalizationService : IService
    {
        IReadOnlyList<Culture> SupportedCultures { get; }
    }
}
