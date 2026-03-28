using Not.Core.Model.Localization;

namespace Not.Core.Model.Metadata.Property
{
    public class LocalizedStringPropertyInfo<BO> : PropertyInfo<BO, LocalizedString>
        where BO : BusinessObject
    {
        public int MaxLength { get; set; }

        public LocalizedStringPropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
