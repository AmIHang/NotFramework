namespace Not.Core.Localization
{
    public class LocalizationService : ILocalizationService
    {
        public IReadOnlyList<Culture> SupportedCultures
            => LocalizationConfig.Current.SupportedCultures;
    }
}
