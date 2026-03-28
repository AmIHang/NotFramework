using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Not.Core.Persistence;
using Not.Core.Tests.Fixtures;
using Not.Sqlite.Persistence;
using Xunit;

namespace Not.Core.Tests;

public class SessionTests
{
    // ── DI / factory helpers ──────────────────────────────────────────────────

    private static IServiceProvider BuildServiceProvider(SqliteMemoryDatabase? db = null, bool addGreetingService = false)
    {
        db ??= new SqliteMemoryDatabase();
        var services = new ServiceCollection();

        services.AddDbContext<TestContext>(opts => db.Configure(opts));

        // Session resolves DatabaseContext from scope; alias TestContext as DatabaseContext
        services.AddScoped<DatabaseContext>(sp => sp.GetRequiredService<TestContext>());

        services.AddSingleton<ISessionFactory, SessionFactory>();

        if (addGreetingService)
            services.AddScoped<IGreetingService, GreetingService>();

        var sp = services.BuildServiceProvider();

        // Ensure the schema exists before tests start writing data
        using var scope = sp.CreateScope();
        scope.ServiceProvider.GetRequiredService<TestContext>().Database.EnsureCreated();

        return sp;
    }

    private static ISession CreateSession(IServiceProvider? sp = null)
    {
        sp ??= BuildServiceProvider();
        return sp.GetRequiredService<ISessionFactory>().Create();
    }

    // ── Current session tracking ──────────────────────────────────────────────

    [Fact]
    public void Create_SetsCurrent_ToNewSession()
    {
        using var session = CreateSession();

        Assert.NotNull(Session.Current);
        Assert.Same(session, Session.Current);
    }

    [Fact]
    public void Dispose_ClearsCurrent()
    {
        var session = CreateSession();

        session.Dispose();

        Assert.Null(Session.Current);
    }

    // ── Query<BO> ─────────────────────────────────────────────────────────────

    [Fact]
    public void QueryGeneric_ReturnsEmptyQueryable_WhenNoDataExists()
    {
        using var session = CreateSession();

        var result = session.Query<Person>().ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void QueryGeneric_ReturnsPersistedEntities()
    {
        using var db = new SqliteMemoryDatabase();
        var sp = BuildServiceProvider(db);

        // Persist via first session
        using (var s1 = sp.GetRequiredService<ISessionFactory>().Create())
        {
            IEntityBroker broker = new EntityBroker(((Session)s1)._dbc);
            var p = broker.Create<Person>();
            p.Name = "Alice";
            s1.Commit();
        }

        // Read via second session
        using var s2 = sp.GetRequiredService<ISessionFactory>().Create();
        var people = s2.Query<Person>().ToList();

        Assert.Single(people);
        Assert.Equal("Alice", people[0].Name);
    }

    [Fact]
    public void Query_ByType_ThrowsException_ForNonBusinessObjectType()
    {
        using var session = CreateSession();

        Assert.Throws<Exception>(() => session.Query(typeof(string)));
    }

    // ── Commit ────────────────────────────────────────────────────────────────

    [Fact]
    public void Commit_PersistsChanges()
    {
        using var db = new SqliteMemoryDatabase();
        var sp = BuildServiceProvider(db);

        using (var s1 = sp.GetRequiredService<ISessionFactory>().Create())
        {
            IEntityBroker broker = new EntityBroker(((Session)s1)._dbc);
            var p = broker.Create<Person>();
            p.Name = "Bob";
            s1.Commit();
        }

        // Verify via a raw context on the same database
        using var ctx = TestContext.CreateInMemory(db);
        Assert.Equal("Bob", ctx.Set<Person>().Single().Name);
    }

    // ── Service resolution ────────────────────────────────────────────────────

    [Fact]
    public void GetService_ReturnsNull_WhenServiceNotRegistered()
    {
        using var session = CreateSession();

        var result = session.GetService(typeof(IGreetingService));

        Assert.Null(result);
    }

    [Fact]
    public void GetRequiredService_ReturnsService_WhenRegistered()
    {
        var sp = BuildServiceProvider(addGreetingService: true);
        using var session = sp.GetRequiredService<ISessionFactory>().Create();

        var svc = session.GetRequiredService(typeof(IGreetingService)) as IGreetingService;

        Assert.NotNull(svc);
        Assert.Equal("Hello from GreetingService", svc.Greet());
    }

    [Fact]
    public void GetRequiredService_Throws_WhenServiceNotRegistered()
    {
        using var session = CreateSession(addGreetingService: false);

        Assert.ThrowsAny<Exception>(() => session.GetRequiredService(typeof(IGreetingService)));
    }

    private static ISession CreateSession(bool addGreetingService)
        => BuildServiceProvider(addGreetingService: addGreetingService)
            .GetRequiredService<ISessionFactory>()
            .Create();

    // ── GetServices ───────────────────────────────────────────────────────────

    [Fact]
    public void GetServices_ReturnsAllRegisteredImplementations()
    {
        using var db = new SqliteMemoryDatabase();
        var services = new ServiceCollection();
        services.AddDbContext<TestContext>(opts => db.Configure(opts));
        services.AddScoped<DatabaseContext>(sp => sp.GetRequiredService<TestContext>());
        services.AddSingleton<ISessionFactory, SessionFactory>();
        services.AddScoped<IGreetingService, GreetingService>();
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        scope.ServiceProvider.GetRequiredService<TestContext>().Database.EnsureCreated();

        using var session = sp.GetRequiredService<ISessionFactory>().Create();
        var result = session.GetServices(typeof(IGreetingService)).ToList();

        Assert.Single(result);
    }
}
