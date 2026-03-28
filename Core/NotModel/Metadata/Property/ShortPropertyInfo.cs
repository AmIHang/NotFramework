namespace Not.Core.Model.Metadata.Property
{
    public class ShortPropertyInfo<BO> : PropertyInfo<BO, short>
        where BO : BusinessObject
    {
        public ShortPropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
