namespace Not.Core.Model.Metadata.Property
{
    public class DoublePropertyInfo<BO> : PropertyInfo<BO, double>
        where BO : BusinessObject
    {
        public DoublePropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
