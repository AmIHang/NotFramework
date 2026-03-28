using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Not.Core.Model.Localization;

namespace Not.Core.EF.Persistence.Model
{
    /// <summary>
    /// EF Core value converter that maps the LocalizedString CLR property to and from
    /// the invariant-culture string column (the "main" column with no language suffix).
    ///
    /// Non-invariant languages are stored in additional shadow properties registered
    /// by CommonEntityTableMappingStrategy and synced by LocalizedStringSync.
    /// </summary>
    internal class LocalizedStringInvariantConverter : ValueConverter<LocalizedString, string>
    {
        public LocalizedStringInvariantConverter() : base(
            ls => Serialize(ls),
            s  => Deserialize(s)
        )
        { }

        private static string Serialize(LocalizedString ls)
            => ls[""];

        private static LocalizedString Deserialize(string s)
        {
            var ls = new LocalizedString();
            ls[""] = s ?? "";
            return ls;
        }
    }
}
