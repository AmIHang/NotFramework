using Not.Core.Model;
using Not.Core.Model.Metadata;
using Not.Core.Tests.Fixtures;
using Xunit;

namespace Not.Core.Tests;

public class ClassInfoTests
{
    // ── Constructor validation ────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_ForNullType()
    {
        Assert.Throws<ArgumentNullException>(() => new NullTypeClassInfo());
    }

    [Fact]
    public void Constructor_ThrowsException_ForTypeNotDerivedFromBusinessObject()
    {
        Assert.Throws<Exception>(() => new NonBusinessObjectClassInfo());
    }

    // ── Basic properties ──────────────────────────────────────────────────────

    [Fact]
    public void Type_ReturnsEntityType()
    {
        Assert.Equal(typeof(Person), Person.ClassInfo.Type);
        Assert.Equal(typeof(Employee), Employee.ClassInfo.Type);
    }

    [Fact]
    public void ClassName_ReturnsSimpleTypeName()
    {
        Assert.Equal("Person", Person.ClassInfo.ClassName);
        Assert.Equal("Employee", Employee.ClassInfo.ClassName);
    }

    // ── TableName ─────────────────────────────────────────────────────────────

    [Fact]
    public void TableName_WithNamespace_PrefixesLastNamespaceSegment()
    {
        // Person is in Not.Core.Tests.Fixtures → last segment = "Fixtures"
        // Expected: "Fixtures" + "Person" = "FixturesPerson"
        Assert.Equal("FixturesPerson", Person.ClassInfo.TableName);
    }

    [Fact]
    public void TableName_WithNamespace_WorksForDerivedEntity()
    {
        Assert.Equal("FixturesEmployee", Employee.ClassInfo.TableName);
    }

    [Fact]
    public void TableName_WithoutNamespace_IsJustClassName()
    {
        // GlobalNamespaceEntity has no namespace (null) → TableName = class name only
        var ci = new GlobalNamespaceEntityClassInfo();
        Assert.Equal(nameof(GlobalNamespaceEntity), ci.TableName);
    }

    // ── IsDerivation ──────────────────────────────────────────────────────────

    [Fact]
    public void IsDerivation_False_ForDirectBusinessObjectChild()
    {
        Assert.False(Person.ClassInfo.IsDerivation);
    }

    [Fact]
    public void IsDerivation_True_ForEntityDerivedFromOtherEntity()
    {
        Assert.True(Employee.ClassInfo.IsDerivation);
    }

    // ── RootType ──────────────────────────────────────────────────────────────

    [Fact]
    public void RootType_ForDirectChild_ReturnsSelf()
    {
        Assert.Equal(typeof(Person), Person.ClassInfo.RootType);
    }

    [Fact]
    public void RootType_ForDerivedEntity_ReturnsDirectBusinessObjectChild()
    {
        // Employee → Person → BusinessObject; RootType should be Person
        Assert.Equal(typeof(Person), Employee.ClassInfo.RootType);
    }

    // ── BaseType ──────────────────────────────────────────────────────────────

    [Fact]
    public void BaseType_ForDirectChild_ReturnsSelf()
    {
        // Person directly extends BusinessObject → BaseType = Person itself
        Assert.Equal(typeof(Person), Person.ClassInfo.BaseType);
    }

    [Fact]
    public void BaseType_ForDerivedEntity_ReturnsImmediateParent()
    {
        Assert.Equal(typeof(Person), Employee.ClassInfo.BaseType);
    }

    // ── RootClassInfo ─────────────────────────────────────────────────────────

    [Fact]
    public void RootClassInfo_ForDirectChild_ReturnsSameInstance()
    {
        Assert.Same(Person.ClassInfo, Person.ClassInfo.RootClassInfo);
    }

    [Fact]
    public void RootClassInfo_ForDerivedEntity_ReturnsRootEntityClassInfo()
    {
        // Employee's root is Person, so RootClassInfo should be Person.ClassInfo
        Assert.Same(Person.ClassInfo, Employee.ClassInfo.RootClassInfo);
    }

    // ── Helpers for error-case testing ───────────────────────────────────────

    // Passes null to the protected ClassInfo(Type) constructor
    private sealed class NullTypeClassInfo : ClassInfo
    {
        public override int CID => 0;
        public override string TableMappingStrategy => "TPH";
        public NullTypeClassInfo() : base(null!) { }
    }

    // Passes a non-BusinessObject type
    private sealed class NonBusinessObjectClassInfo : ClassInfo
    {
        public override int CID => 0;
        public override string TableMappingStrategy => "TPH";
        public NonBusinessObjectClassInfo() : base(typeof(string)) { }
    }

    // Wraps GlobalNamespaceEntity whose namespace is null (declared at global scope)
    private sealed class GlobalNamespaceEntityClassInfo : ClassInfo
    {
        public override int CID => 0;
        public override string TableMappingStrategy => "TPH";
        public GlobalNamespaceEntityClassInfo() : base(typeof(GlobalNamespaceEntity)) { }
    }
}
