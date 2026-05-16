using System.Globalization;
using Not.Core.Yaml.Features;
using Xunit;

namespace Not.Core.Tests;

public class YamlFeatureLoaderTests
{
    private readonly YamlFeatureLoader _loader = new();

    [Fact]
    public void Load_WithTitleAndHint_ReturnsFeatureDefinition()
    {
        var yaml = """
            Name:
              title:
                "-": "Full Name"
                de: "Vollständiger Name"
              hint:
                "-": "The person's full name"
                de: "Der vollständige Name"
            """;

        var features = _loader.Load(yaml);

        Assert.Single(features);
        Assert.Equal("Name", features[0].Name);
        Assert.Equal("Full Name", features[0].Title[""]);
        Assert.Equal("Vollständiger Name", features[0].Title["de"]);
        Assert.Equal("The person's full name", features[0].Hint![""] );
        Assert.Equal("Der vollständige Name", features[0].Hint!["de"]);
    }

    [Fact]
    public void Load_DashKeyMapsToInvariantCultureCode()
    {
        var yaml = """
            Age:
              title:
                "-": "Age"
            """;

        var features = _loader.Load(yaml);

        Assert.Equal("Age", features[0].Title[""]);
    }

    [Fact]
    public void Load_MissingCultureFallsBackToInvariant()
    {
        var yaml = """
            Name:
              title:
                "-": "Fallback"
            """;

        var features = _loader.Load(yaml);
        var title = features[0].Title;

        RunWithCulture("fr", () => Assert.Equal("Fallback", title.Value));
    }

    [Fact]
    public void Load_HintIsOptional()
    {
        var yaml = """
            Age:
              title:
                "-": "Age"
            """;

        var features = _loader.Load(yaml);

        Assert.Null(features[0].Hint);
    }

    [Fact]
    public void Load_MultipleFeatures_ReturnsAll()
    {
        var yaml = """
            Name:
              title:
                "-": "Name"
            Age:
              title:
                "-": "Age"
                de: "Alter"
            """;

        var features = _loader.Load(yaml);

        Assert.Equal(2, features.Count);
        Assert.Contains(features, f => f.Name == "Name");
        Assert.Contains(features, f => f.Name == "Age");
        Assert.Equal("Alter", features.First(f => f.Name == "Age").Title["de"]);
    }

    [Fact]
    public void Load_EmptyYaml_ReturnsEmptyList()
    {
        var features = _loader.Load("{}");

        Assert.Empty(features);
    }

    [Fact]
    public void Load_InvariantValueUsedWhenCurrentCulturePresent()
    {
        var yaml = """
            Name:
              title:
                "-": "Name"
                de: "Name (DE)"
            """;

        var features = _loader.Load(yaml);
        var title = features[0].Title;

        RunWithCulture("de", () => Assert.Equal("Name (DE)", title.Value));
        RunWithCulture("en", () => Assert.Equal("Name", title.Value));
    }

    private static void RunWithCulture(string cultureCode, Action action)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(cultureCode);
            action();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
