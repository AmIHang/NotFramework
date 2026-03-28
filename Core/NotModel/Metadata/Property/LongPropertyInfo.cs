namespace Not.Core.Model.Metadata.Property
{
    public class LongPropertyInfo<BO> : PropertyInfo<BO, long>
        where BO : BusinessObject
    {
        public LongPropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
