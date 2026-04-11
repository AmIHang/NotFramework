using Microsoft.EntityFrameworkCore;
using Not.Core.EF.Persistence.Model;
using Not.Core.Model.Metadata;
using Not.Core.Persistence;
using Not.Sqlite.Persistence;

namespace Not.Core.Tests.Fixtures;

/// <summary>
/// Minimal DatabaseContext for EntityBroker and Session tests.
/// Registers Person and Employee via explicit model configuration.
/// </summary>
public class TestContext : DatabaseContext
{
    // Non-null only when this context created (and therefore owns) the database.
    private SqliteMemoryDatabase? _ownedDb;

    public TestContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(e =>
        {
            e.HasKey(x => x.OID);
            e.Property(x => x.Name);
            e.Property(x => x.Age);
            e.Ignore(x => x.Title);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.HasBaseType<Person>();
            e.Property(x => x.Department);
        });
    }

    public override void Dispose()
    {
        base.Dispose();
        _ownedDb?.Dispose();
        _ownedDb = null;
    }

    /// <summary>
    /// Creates a standalone in-memory SQLite context with its own private
    /// database. The schema is created automatically.
    /// </summary>
    public static TestContext CreateInMemory()
    {
        var db = new SqliteMemoryDatabase();
        var options = db.Configure(new DbContextOptionsBuilder<TestContext>()).Options;
        var ctx = new TestContext(options) { _ownedDb = db };
        ctx.Database.EnsureCreated();
        return ctx;
    }

    /// <summary>
    /// Creates a context that shares the given <paramref name="db"/> connection,
    /// allowing data written by one context to be visible in another.
    /// The schema is created if it does not yet exist.
    /// </summary>
    public static TestContext CreateInMemory(SqliteMemoryDatabase db)
    {
        var options = db.Configure(new DbContextOptionsBuilder<TestContext>()).Options;
        var ctx = new TestContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}

/// <summary>
/// ModelDefinition for relationship property tests.
/// Includes Person, Order, OrderItem, Vehicle and Car.
/// </summary>
public class RelationshipModelDefinition : ModelDefinition
{
    private static readonly Guid _guid = Guid.Parse("D4E5F6A7-B8C9-0123-DEFA-456789ABCDEF");

    public override Guid Guid => _guid;
    public override string Name => "RelationshipModel";
    public override string Version => "1.0.0";

    public RelationshipModelDefinition(DbContextOptions options) : base(options) { }

    protected override IEnumerable<ClassInfo> GetClassInfos()
    {
        yield return Person.ClassInfo;
        yield return Order.ClassInfo;
        yield return OrderItem.ClassInfo;
        yield return Vehicle.ClassInfo;
        yield return Car.ClassInfo;
        yield return Supplier.ClassInfo;
        yield return SupplierContact.ClassInfo;
    }

    public static RelationshipModelDefinition CreateInMemory()
    {
        var db = new SqliteMemoryDatabase();
        var options = db.Configure(new DbContextOptionsBuilder<RelationshipModelDefinition>()).Options;
        return new RelationshipModelDefinition(options);
    }

    public static (RelationshipModelDefinition ctx, SqliteMemoryDatabase db) CreateInMemoryWithDb()
    {
        var db = new SqliteMemoryDatabase();
        var options = db.Configure(new DbContextOptionsBuilder<RelationshipModelDefinition>()).Options;
        return (new RelationshipModelDefinition(options), db);
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

    /// <summary>
    /// Creates a standalone in-memory SQLite context for metadata tests.
    /// Schema is <b>not</b> automatically created; call
    /// <c>Database.EnsureCreated()</c> explicitly when needed.
    /// </summary>
    public static TestModelDefinition CreateInMemory()
    {
        var db = new SqliteMemoryDatabase();
        var options = db.Configure(new DbContextOptionsBuilder<TestModelDefinition>()).Options;
        return new TestModelDefinition(options);
    }
}
