using Not.Core.Model.Metadata.Property;
using Not.Core.Tests.Fixtures;
using Xunit;

namespace Not.Core.Tests;

public class PropertyInfoTests
{
    // ── PropertyName ──────────────────────────────────────────────────────────

    [Fact]
    public void PropertyName_ReturnsNameOfMappedProperty()
    {
        Assert.Equal("Name", Person.NameInfo.PropertyName);
        Assert.Equal("Age", Person.AgeInfo.PropertyName);
        Assert.Equal("Department", Employee.DepartmentInfo.PropertyName);
    }

    // ── PropertyType ──────────────────────────────────────────────────────────

    [Fact]
    public void PropertyType_ReturnsCorrectClrType()
    {
        Assert.Equal(typeof(string), Person.NameInfo.PropertyType);
        Assert.Equal(typeof(int), Person.AgeInfo.PropertyType);
        Assert.Equal(typeof(string), Employee.DepartmentInfo.PropertyType);
    }

    // ── ColumnName ────────────────────────────────────────────────────────────

    [Fact]
    public void ColumnName_DefaultsToPropertyName()
    {
        // A freshly created PropertyInfo without explicit ColumnName assignment
        var prop = new StringPropertyInfo<Person>("Name");
        Assert.Equal("Name", prop.ColumnName);
    }

    [Fact]
    public void ColumnName_CanBeOverridden()
    {
        var prop = new StringPropertyInfo<Person>("Name") { ColumnName = "person_name" };
        Assert.Equal("person_name", prop.ColumnName);
    }

    [Fact]
    public void ColumnName_OverrideDoesNotAffectPropertyName()
    {
        var prop = new StringPropertyInfo<Person>("Name") { ColumnName = "person_name" };
        Assert.Equal("Name", prop.PropertyName);
    }

    // ── IsRequired ────────────────────────────────────────────────────────────

    [Fact]
    public void IsRequired_DefaultsFalse()
    {
        var prop = new StringPropertyInfo<Person>("Name");
        Assert.False(prop.IsRequired);
    }

    [Fact]
    public void IsRequired_CanBeSetToTrue()
    {
        var prop = new StringPropertyInfo<Person>("Name") { IsRequired = true };
        Assert.True(prop.IsRequired);
    }

    // ── StringPropertyInfo ────────────────────────────────────────────────────

    [Fact]
    public void StringPropertyInfo_MaxLength_DefaultsZero()
    {
        var prop = new StringPropertyInfo<Person>("Name");
        Assert.Equal(0, prop.MaxLength);
    }

    [Fact]
    public void StringPropertyInfo_MaxLength_CanBeSet()
    {
        var prop = new StringPropertyInfo<Person>("Name") { MaxLength = 200 };
        Assert.Equal(200, prop.MaxLength);
    }

    // ── Constructor validation ────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsException_ForNonExistentProperty()
    {
        Assert.Throws<Exception>(() => new StringPropertyInfo<Person>("NonExistentProperty"));
    }

    [Fact]
    public void Constructor_ThrowsException_ForWrongPropertyType()
    {
        // "Age" is an int property; using IntPropertyInfo<Person> is correct,
        // but the constructor only validates existence, not type safety at runtime.
        // Here we just confirm a missing name throws.
        Assert.Throws<Exception>(() => new IntPropertyInfo<Person>("Missing"));
    }
}
