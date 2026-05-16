using Not.Core.Model.Localization;

namespace Not.Core.Model.Features;

public class FeatureDefinition
{
    public string Name { get; }
    public LocalizedString Title { get; }
    public LocalizedString? Hint { get; }

    public FeatureDefinition(string name, LocalizedString title, LocalizedString? hint = null)
    {
        Name = name;
        Title = title;
        Hint = hint;
    }
}
