using Microsoft.EntityFrameworkCore;
using Not.Core.EF.Persistence.Model;
using Not.Core.Model.Metadata;
using Not.Core.Persistence;

namespace Not.Core.Tests.Fixtures;

/// <summary>
/// Minimal DatabaseContext for EntityBroker and Session tests.
/// Registers Person and Employee via explicit model configuration.
/// </summary>
public class TestContext : DatabaseContext
{
    public TestContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(e =>
        {
            e.HasKey(x => x.OID);
            e.Property(x => x.Name);
            e.Property(x => x.Age);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.HasBaseType<Person>();
            e.Property(x => x.Department);
        });
    }

    public static TestContext CreateInMemory(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<TestContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new TestContext(options);
    }
}

/// <summary>
/// ModelDefinition implementation for ModelDefinition tests.
/// Uses TPH strategy for both Person and Employee.
/// </summary>
public class TestModelDefinition : ModelDefinition
{
    private static readonly Guid _guid = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

    public override Guid Guid => _guid;
    public override string Name => "TestModel";
    public override string Version => "1.0.0";

    public TestModelDefinition(DbContextOptions options) : base(options) { }

    protected override IEnumerable<ClassInfo> GetClassInfos()
    {
        yield return Person.ClassInfo;
        yield return Employee.ClassInfo;
    }

    public static TestModelDefinition CreateInMemory(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<TestModelDefinition>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new TestModelDefinition(options);
    }
}
