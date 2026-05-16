using Not.Core.Model.Features;
using Not.Core.Model.Localization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Not.Core.Yaml.Features;

public class YamlFeatureLoader
{
    private static readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(LowerCaseNamingConvention.Instance)
        .Build();

    public IReadOnlyList<FeatureDefinition> Load(string yaml)
    {
        var raw = _deserializer.Deserialize<Dictionary<string, RawFeature>?>(yaml);
        if (raw == null)
            return [];

        return raw.Select(kv => ToDefinition(kv.Key, kv.Value)).ToList();
    }

    private static FeatureDefinition ToDefinition(string name, RawFeature raw)
    {
        var title = ToLocalizedString(raw.Title ?? []);
        var hint = raw.Hint != null ? ToLocalizedString(raw.Hint) : null;
        return new FeatureDefinition(name, title, hint);
    }

    private static LocalizedString ToLocalizedString(Dictionary<string, string> dict)
    {
        var mapped = dict.ToDictionary(
            kv => kv.Key == "-" ? "" : kv.Key,
            kv => kv.Value);
        return new LocalizedString(mapped);
    }

    private class RawFeature
    {
        public Dictionary<string, string>? Title { get; set; }
        public Dictionary<string, string>? Hint { get; set; }
    }
}
