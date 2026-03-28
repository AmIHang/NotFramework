namespace Not.Core.Model.Metadata.Property
{
    public interface IDecimalProperty
    {
        int Precision { get; }
        int Scale { get; }
    }

    public class DecimalPropertyInfo<BO> : PropertyInfo<BO, decimal>, IDecimalProperty
        where BO : BusinessObject
    {
        public int Precision { get; set; } = 18;
        public int Scale { get; set; } = 2;

        public DecimalPropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
