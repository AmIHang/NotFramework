using Not.Core.Model;
using Not.Core.Persistence;
using Not.Core.Tests.Fixtures;
using Not.Sqlite.Persistence;
using Xunit;

namespace Not.Core.Tests;

public class EntityBrokerTests
{
    // Create<BO>() is a default interface method on IEntityBroker, so the variable
    // must be typed as IEntityBroker (not the concrete EntityBroker class).
    private static (TestContext ctx, IEntityBroker broker) CreateBroker(SqliteMemoryDatabase? db = null)
    {
        var ctx = db is null ? TestContext.CreateInMemory() : TestContext.CreateInMemory(db);
        IEntityBroker broker = new EntityBroker(ctx);
        return (ctx, broker);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_ForNullContext()
    {
        Assert.Throws<ArgumentNullException>(() => new EntityBroker(null!));
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ReturnsInstanceOfRequestedType()
    {
        var (_, broker) = CreateBroker();

        var person = broker.Create(typeof(Person));

        Assert.NotNull(person);
        Assert.IsType<Person>(person);
    }

    [Fact]
    public void Create_Generic_ReturnsStronglyTypedInstance()
    {
        var (_, broker) = CreateBroker();

        var person = broker.Create<Person>();

        Assert.NotNull(person);
        Assert.IsType<Person>(person);
    }

    [Fact]
    public void Create_WorksForDerivedEntity()
    {
        var (_, broker) = CreateBroker();

        var employee = broker.Create<Employee>();

        Assert.NotNull(employee);
        Assert.IsType<Employee>(employee);
    }

    [Fact]
    public void Create_ThrowsArgumentNullException_ForNullType()
    {
        var (_, broker) = CreateBroker();

        Assert.Throws<ArgumentNullException>(() => broker.Create(null!));
    }

    [Fact]
    public void Create_ThrowsException_ForTypeNotDerivedFromBusinessObject()
    {
        var (_, broker) = CreateBroker();

        Assert.Throws<Exception>(() => broker.Create(typeof(string)));
    }

    [Fact]
    public void Create_TracksNewEntityAsAdded()
    {
        var (_, broker) = CreateBroker();

        var person = broker.Create<Person>();

        Assert.Equal(EntityState.Added, broker.GetEntityState(person));
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ThrowsArgumentNullException_ForNullEntity()
    {
        var (_, broker) = CreateBroker();

        Assert.Throws<ArgumentNullException>(() => broker.Delete(null!));
    }

    [Fact]
    public void Delete_MarksEntityAsDeleted()
    {
        // Entity must be saved first; EF Core transitions an unsaved Added entity
        // to Detached (not Deleted) when removed before SaveChanges.
        var (ctx, broker) = CreateBroker();
        var person = broker.Create<Person>();
        person.Name = "ToDelete";
        ctx.SaveChanges(); // state = Current

        broker.Delete(person);

        Assert.Equal(EntityState.Deleted, broker.GetEntityState(person));
    }

    [Fact]
    public void Delete_UnsavedEntity_BecomesUntracked()
    {
        // EF Core specific: removing a never-saved (Added) entity detaches it.
        var (_, broker) = CreateBroker();
        var person = broker.Create<Person>();

        broker.Delete(person);

        // After removing an Added entity, it is Detached → GetEntityState throws
        Assert.Throws<Exception>(() => broker.GetEntityState(person));
    }

    // ── GetEntityState ────────────────────────────────────────────────────────

    [Fact]
    public void GetEntityState_ThrowsArgumentNullException_ForNullEntity()
    {
        var (_, broker) = CreateBroker();

        Assert.Throws<ArgumentNullException>(() => broker.GetEntityState(null!));
    }

    [Fact]
    public void GetEntityState_ThrowsException_ForUntrackedEntity()
    {
        var (_, broker) = CreateBroker();
        var untracked = new Person { Name = "Ghost" };

        Assert.Throws<Exception>(() => broker.GetEntityState(untracked));
    }

    [Fact]
    public void GetEntityState_ReturnsAdded_ForNewlyCreatedEntity()
    {
        var (_, broker) = CreateBroker();
        var person = broker.Create<Person>();

        Assert.Equal(EntityState.Added, broker.GetEntityState(person));
    }

    [Fact]
    public void GetEntityState_ReturnsCurrent_AfterCommit()
    {
        var (ctx, broker) = CreateBroker();
        var person = broker.Create<Person>();
        person.Name = "Alice";
        ctx.SaveChanges();

        Assert.Equal(EntityState.Current, broker.GetEntityState(person));
    }

    [Fact]
    public void GetEntityState_ReturnsModified_AfterPropertyChange()
    {
        var (ctx, broker) = CreateBroker();
        var person = broker.Create<Person>();
        person.Name = "Alice";
        ctx.SaveChanges();

        person.Name = "Bob";

        Assert.Equal(EntityState.Modified, broker.GetEntityState(person));
    }

    [Fact]
    public void GetEntityState_ReturnsDeleted_AfterDelete()
    {
        var (ctx, broker) = CreateBroker();
        var person = broker.Create<Person>();
        person.Name = "Alice";
        ctx.SaveChanges();

        broker.Delete(person);

        Assert.Equal(EntityState.Deleted, broker.GetEntityState(person));
    }

    // ── Multiple entities in same context ─────────────────────────────────────

    [Fact]
    public void Create_MultipleEntities_AreTrackedIndependently()
    {
        var (ctx, broker) = CreateBroker();

        var p1 = broker.Create<Person>();
        var p2 = broker.Create<Person>();
        p1.Name = "Alice";
        p2.Name = "Bob";
        ctx.SaveChanges();

        Assert.Equal(EntityState.Current, broker.GetEntityState(p1));
        Assert.Equal(EntityState.Current, broker.GetEntityState(p2));
    }

    [Fact]
    public void Commit_PersistsEntities_ToInMemoryStore()
    {
        using var db = new SqliteMemoryDatabase();
        var (ctx, broker) = CreateBroker(db);

        var person = broker.Create<Person>();
        person.Name = "Alice";
        person.Age = 30;
        ctx.SaveChanges();
        ctx.Dispose();

        // Open a second context on the same in-memory database
        using var ctx2 = TestContext.CreateInMemory(db);
        var loaded = ctx2.Set<Person>().Single();
        Assert.Equal("Alice", loaded.Name);
        Assert.Equal(30, loaded.Age);
    }
}
