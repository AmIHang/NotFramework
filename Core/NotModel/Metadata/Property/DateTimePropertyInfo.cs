namespace Not.Core.Model.Metadata.Property
{
    public class DateTimePropertyInfo<BO> : PropertyInfo<BO, DateTime>
        where BO : BusinessObject
    {
        public DateTimePropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
