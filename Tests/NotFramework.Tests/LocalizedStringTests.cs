using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Not.Core.EF.Persistence.Model;
using Not.Core.Model.Localization;
using Not.Core.Model.Metadata.Property;
using Not.Core.Tests.Fixtures;
using Xunit;

namespace Not.Core.Tests;

public class LocalizedStringTests
{
    // ── Value (thread culture) ────────────────────────────────────────────────

    [Fact]
    public void Value_Get_ReturnsValueForCurrentUICulture()
    {
        var ls = new LocalizedString();
        ls["de"] = "Hallo";
        ls["en"] = "Hello";

        RunWithCulture("de", () => Assert.Equal("Hallo", ls.Value));
        RunWithCulture("en", () => Assert.Equal("Hello", ls.Value));
    }

    [Fact]
    public void Value_Set_StoresUnderCurrentUICulture()
    {
        var ls = new LocalizedString();

        RunWithCulture("de", () => ls.Value = "Hallo");
        RunWithCulture("en", () => ls.Value = "Hello");

        Assert.Equal("Hallo", ls["de"]);
        Assert.Equal("Hello", ls["en"]);
    }

    [Fact]
    public void Value_Get_FallsBackToInvariantWhenCultureMissing()
    {
        var ls = new LocalizedString();
        ls[""] = "Fallback";

        RunWithCulture("fr", () => Assert.Equal("Fallback", ls.Value));
    }

    [Fact]
    public void Value_Get_ReturnsEmptyStringWhenNoValueAndNoFallback()
    {
        var ls = new LocalizedString();
        RunWithCulture("de", () => Assert.Equal("", ls.Value));
    }

    // ── Indexer ───────────────────────────────────────────────────────────────

    [Fact]
    public void Indexer_SetAndGet_WorksForArbitraryCultureCode()
    {
        var ls = new LocalizedString();
        ls["fr"] = "Bonjour";
        Assert.Equal("Bonjour", ls["fr"]);
    }

    [Fact]
    public void Indexer_Get_ReturnsEmptyStringForMissingCode()
    {
        var ls = new LocalizedString();
        Assert.Equal("", ls["xx"]);
    }

    [Fact]
    public void Indexer_IsCaseInsensitive()
    {
        var ls = new LocalizedString();
        ls["DE"] = "Hallo";
        Assert.Equal("Hallo", ls["de"]);
    }

    // ── Implicit conversions ──────────────────────────────────────────────────

    [Fact]
    public void ImplicitFromString_SetsValueForCurrentUICulture()
    {
        RunWithCulture("de", () =>
        {
            LocalizedString ls = "Hallo";
            Assert.Equal("Hallo", ls["de"]);
        });
    }

    [Fact]
    public void ImplicitToString_ReturnsCurrentCultureValue()
    {
        var ls = new LocalizedString();
        ls["en"] = "Hello";

        RunWithCulture("en", () =>
        {
            string s = ls;
            Assert.Equal("Hello", s);
        });
    }

    // ── Translations ──────────────────────────────────────────────────────────

    [Fact]
    public void Translations_ReturnsAllStoredValues()
    {
        var ls = new LocalizedString();
        ls["de"] = "Hallo";
        ls["en"] = "Hello";

        Assert.Equal(2, ls.Translations.Count);
        Assert.Equal("Hallo", ls.Translations["de"]);
        Assert.Equal("Hello", ls.Translations["en"]);
    }

    [Fact]
    public void Constructor_WithDictionary_CopiesValues()
    {
        var dict = new Dictionary<string, string> { ["de"] = "Hallo", [""] = "Fallback" };
        var ls = new LocalizedString(dict);

        Assert.Equal("Hallo", ls["de"]);
        Assert.Equal("Fallback", ls[""]);
    }

    // ── LocalizedStringPropertyInfo ───────────────────────────────────────────

    [Fact]
    public void LocalizedStringPropertyInfo_PropertyType_IsLocalizedString()
    {
        Assert.Equal(typeof(LocalizedString), Person.TitleInfo.PropertyType);
    }

    [Fact]
    public void LocalizedStringPropertyInfo_PropertyName_IsCorrect()
    {
        Assert.Equal("Title", Person.TitleInfo.PropertyName);
    }

    [Fact]
    public void LocalizedStringPropertyInfo_MaxLength_DefaultsZero()
    {
        Assert.Equal(0, Person.TitleInfo.MaxLength);
    }

    // ── EF Core round-trip via ModelDefinition ────────────────────────────────

    [Fact]
    public async Task LocalizedString_RoundTrip_ThroughModelDefinition()
    {
        await using var ctx = TestModelDefinition.CreateInMemory();
        await ctx.Database.EnsureCreatedAsync();

        var person = new Person { Name = "Alice", Age = 30 };
        person.Title["de"] = "Frau";

        ctx.Set<Person>().Add(person);
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();

        var loaded = await ctx.Set<Person>().FirstAsync(x => x.Name == "Alice");
        Assert.Equal("Frau", loaded.Title["de"]);
    }

    [Fact]
    public async Task LocalizedString_EachCultureMappedToOwnShadowProperty()
    {
        await using var ctx = TestModelDefinition.CreateInMemory();
        await ctx.Database.EnsureCreatedAsync();

        var person = new Person { Name = "Bob", Age = 25 };
        person.Title["de"] = "Herr";

        ctx.Set<Person>().Add(person);
        await ctx.SaveChangesAsync();

        // Shadow properties exist for each non-invariant configured culture (German by default).
        // The invariant value is stored in the CLR property column ("Title") via value converter.
        var entry = ctx.Entry(person);
        var shadowNames = entry.Properties
            .Where(p => p.Metadata.IsShadowProperty() && p.Metadata.Name.StartsWith("Title__"))
            .Select(p => p.Metadata.Name)
            .ToList();

        Assert.Contains("Title__de", shadowNames);
        Assert.Equal("Herr", entry.Property("Title__de").CurrentValue);
    }

    [Fact]
    public async Task LocalizedString_ImplicitStringAssignment_SetCurrentCulture()
    {
        RunWithCulture("de", () =>
        {
            var person = new Person { Name = "Test", Age = 1 };
            // implicit assignment via string sets current culture
            person.Title = "Titel";
            Assert.Equal("Titel", person.Title["de"]);
        });
    }

    [Fact]
    public async Task LocalizedString_ImplicitStringGet_ReturnsCurrentCulture()
    {
        var person = new Person { Name = "Test", Age = 1 };
        person.Title["de"] = "Titel";

        RunWithCulture("de", () =>
        {
            string title = person.Title;
            Assert.Equal("Titel", title);
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

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
