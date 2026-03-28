namespace Not.Core.Model.Metadata.Property
{
    public class BytePropertyInfo<BO> : PropertyInfo<BO, byte>
        where BO : BusinessObject
    {
        public BytePropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
