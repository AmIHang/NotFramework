using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Not.Core.Model.Localization;

namespace Not.Core.EF.Persistence.Model
{
    /// <summary>
    /// Helpers for mapping LocalizedString CLR properties to/from their per-language shadow properties.
    ///
    /// Shadow property naming: {PropertyName}__{cultureCode}  e.g. "Title__de", "Title__en"
    ///   Only non-invariant (non-empty code) cultures are stored as shadow properties.
    ///   The invariant culture (code "") is stored in the CLR property column via the
    ///   LocalizedStringInvariantConverter.
    ///
    /// Column naming:
    ///   invariant  →  {ColumnName}          (CLR property, no suffix)
    ///   other      →  {ColumnName}_{code}   e.g. "Title_de"
    /// </summary>
    internal static class LocalizedStringSync
    {
        private const string Separator = "__";

        internal static string GetShadowPropName(string propertyName, string cultureCode)
        => propertyName + Separator + cultureCode;

        internal static string GetColumnName(string baseColumnName, string cultureCode)
            => cultureCode == "" ? baseColumnName : $"{baseColumnName}_{cultureCode}";

        /// <summary>
        /// Copies values from LocalizedString CLR properties into their shadow properties.
        /// Call before SaveChanges for Added/Modified entries.
        /// </summary>
        internal static void SyncToShadow(EntityEntry entry)
        {
            foreach (var clrProp in entry.Entity.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(LocalizedString)))
            {
                if (clrProp.GetValue(entry.Entity) is not LocalizedString ls)
                    continue;

                var prefix = clrProp.Name + Separator;
                foreach (var shadow in entry.Properties
                    .Where(p => p.Metadata.IsShadowProperty() && p.Metadata.Name.StartsWith(prefix)))
                {
                    shadow.CurrentValue = ls[SuffixToCultureCode(shadow.Metadata.Name[prefix.Length..])];
                }
            }
        }

        /// <summary>
        /// Copies shadow property values into LocalizedString CLR properties.
        /// Call after an entity is loaded from the database.
        /// </summary>
        internal static void SyncFromShadow(EntityEntry entry)
        {
            foreach (var clrProp in entry.Entity.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(LocalizedString)))
            {
                if (clrProp.GetValue(entry.Entity) is not LocalizedString ls)
                    continue;

                var prefix = clrProp.Name + Separator;
                foreach (var shadow in entry.Properties
                    .Where(p => p.Metadata.IsShadowProperty() && p.Metadata.Name.StartsWith(prefix)))
                {
                    if (shadow.CurrentValue is string s)
                        ls[SuffixToCultureCode(shadow.Metadata.Name[prefix.Length..])] = s;
                }
            }
        }

        private static string SuffixToCultureCode(string suffix)
            => suffix;  // non-invariant only; invariant is handled via CLR property + converter
    }
}
