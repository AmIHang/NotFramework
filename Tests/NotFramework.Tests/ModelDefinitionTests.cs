using Microsoft.EntityFrameworkCore;
using Not.Core.Tests.Fixtures;
using Xunit;

namespace Not.Core.Tests;

public class ModelDefinitionTests
{
    // ── ClassInfos aggregation ────────────────────────────────────────────────

    [Fact]
    public void ClassInfos_ContainsAllRegisteredClassInfos()
    {
        using var ctx = TestModelDefinition.CreateInMemory();

        var classInfos = ctx.ClassInfos.ToList();

        Assert.Contains(classInfos, ci => ci.Type == typeof(Person));
        Assert.Contains(classInfos, ci => ci.Type == typeof(Employee));
    }

    [Fact]
    public void ClassInfos_CountMatchesRegisteredEntities()
    {
        using var ctx = TestModelDefinition.CreateInMemory();

        Assert.Equal(2, ctx.ClassInfos.Count());
    }

    // ── Include ───────────────────────────────────────────────────────────────

    [Fact]
    public void Include_AddsClassInfosFromDependency()
    {
        using var host = TestModelDefinition.CreateInMemory();
        using var dep = new SecondTestModelDefinition(
            new DbContextOptionsBuilder<SecondTestModelDefinition>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        host.Include(dep);

        var types = host.ClassInfos.Select(ci => ci.Type).ToList();
        Assert.Contains(typeof(Person), types);
        Assert.Contains(typeof(Employee), types);
        Assert.Contains(typeof(Address), types);
    }

    [Fact]
    public void Include_ThrowsException_WhenSameModuleAddedTwice()
    {
        using var host = TestModelDefinition.CreateInMemory();
        using var dep = new SecondTestModelDefinition(
            new DbContextOptionsBuilder<SecondTestModelDefinition>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        host.Include(dep);

        Assert.Throws<Exception>(() => host.Include(dep));
    }

    // ── Model building / EF Core integration ─────────────────────────────────

    [Fact]
    public void OnModelCreating_RegistersEntitiesForQuerying()
    {
        using var ctx = TestModelDefinition.CreateInMemory();
        ctx.Database.EnsureCreated();

        // If the model was built correctly we can query without exceptions
        var people = ctx.Set<Person>().ToList();
        var employees = ctx.Set<Employee>().ToList();

        Assert.Empty(people);
        Assert.Empty(employees);
    }

    [Fact]
    public void OnModelCreating_ThrowsException_WhenClassInfoHasUnknownMappingStrategy()
    {
        var options = new DbContextOptionsBuilder<BadStrategyModelDefinition>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var ctx = new BadStrategyModelDefinition(options);

        // EnsureCreated triggers OnModelCreating
        Assert.Throws<Exception>(() => ctx.Database.EnsureCreated());
    }

    [Fact]
    public void Metadata_Guid_Name_Version_AreExposed()
    {
        using var ctx = TestModelDefinition.CreateInMemory();

        Assert.Equal(Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890"), ctx.Guid);
        Assert.Equal("TestModel", ctx.Name);
        Assert.Equal("1.0.0", ctx.Version);
    }
}

// ── Additional fixtures used only within ModelDefinitionTests ────────────────

public class Address : Not.Core.Model.BusinessObject
{
    public string Street { get; set; } = "";

    public static readonly AddressClassInfo ClassInfo = new();
    public static readonly Not.Core.Model.Metadata.Property.StringPropertyInfo<Address> StreetInfo = new("Street");
}

public class AddressClassInfo : Not.Core.Model.Metadata.ClassInfo<Address>
{
    public override int CID => 200;
    public override string TableMappingStrategy => "TPH";
}

/// <summary>Second model that owns Address, used for Include tests.</summary>
public class SecondTestModelDefinition : Not.Core.EF.Persistence.Model.ModelDefinition
{
    private static readonly Guid _guid = Guid.Parse("B2C3D4E5-F6A7-8901-BCDE-F23456789012");

    public override Guid Guid => _guid;
    public override string Name => "SecondModel";
    public override string Version => "1.0.0";

    public SecondTestModelDefinition(DbContextOptions options) : base(options) { }

    protected override IEnumerable<Not.Core.Model.Metadata.ClassInfo> GetClassInfos()
    {
        yield return Address.ClassInfo;
    }
}

/// <summary>Model with a ClassInfo that returns an unknown mapping strategy.</summary>
public class BadStrategyModelDefinition : Not.Core.EF.Persistence.Model.ModelDefinition
{
    private static readonly Guid _guid = Guid.Parse("C3D4E5F6-A7B8-9012-CDEF-123456789ABC");

    public override Guid Guid => _guid;
    public override string Name => "BadModel";
    public override string Version => "1.0.0";

    public BadStrategyModelDefinition(DbContextOptions options) : base(options) { }

    protected override IEnumerable<Not.Core.Model.Metadata.ClassInfo> GetClassInfos()
    {
        yield return BadEntity.ClassInfo;
    }
}

public class BadEntity : Not.Core.Model.BusinessObject
{
    public static readonly BadEntityClassInfo ClassInfo = new();
}

public class BadEntityClassInfo : Not.Core.Model.Metadata.ClassInfo<BadEntity>
{
    public override int CID => 999;
    public override string TableMappingStrategy => "UNKNOWN_STRATEGY";
}
