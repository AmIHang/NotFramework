using System.Globalization;

namespace Not.Core.Model.Localization
{
    /// <summary>
    /// A string value type that stores translations per culture code.
    /// The default getter/setter uses the current thread's UI culture.
    /// Supported languages are defined by LocalizationConfig in NotCore.
    /// </summary>
    public class LocalizedString
    {
        private readonly Dictionary<string, string> _values;

        public LocalizedString()
        {
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public LocalizedString(Dictionary<string, string> values)
        {
            _values = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets the value for the current thread's UI culture.
        /// Falls back to the invariant culture value if no match is found.
        /// </summary>
        public string Value
        {
            get
            {
                var code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                if (_values.TryGetValue(code, out var value))
                    return value;
                // fallback: invariant (empty code)
                return _values.TryGetValue("", out var invariant) ? invariant : "";
            }
            set
            {
                _values[CultureInfo.CurrentUICulture.TwoLetterISOLanguageName] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value for a specific culture code (e.g. "de", "en", "").
        /// </summary>
        public string this[string cultureCode]
        {
            get => _values.TryGetValue(cultureCode, out var v) ? v : "";
            set => _values[cultureCode] = value;
        }

        /// <summary>
        /// All stored translations, keyed by culture code.
        /// </summary>
        public IReadOnlyDictionary<string, string> Translations => _values;

        public static implicit operator string(LocalizedString ls) => ls.Value;

        public static implicit operator LocalizedString(string value)
        {
            var ls = new LocalizedString();
            ls.Value = value;
            return ls;
        }

        public override string ToString() => Value;
    }
}
