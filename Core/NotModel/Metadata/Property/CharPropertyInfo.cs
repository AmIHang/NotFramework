namespace Not.Core.Model.Metadata.Property
{
    public class CharPropertyInfo<BO> : PropertyInfo<BO, char>
        where BO : BusinessObject
    {
        public CharPropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
