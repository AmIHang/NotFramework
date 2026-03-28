using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Not.Sqlite.Persistence;

/// <summary>
/// Manages a persistent SQLite in-memory connection so that multiple DbContext
/// instances can share the same data within a test or seeding scenario.
///
/// Unlike a connection-string-based in-memory database, SQLite destroys the
/// in-memory store as soon as the last connection to it closes. This class
/// keeps one connection open for its entire lifetime, so contexts created with
/// <see cref="Configure"/> share the same data even after individual contexts
/// are disposed.
///
/// Usage:
/// <code>
/// using var db = new SqliteMemoryDatabase();
/// using var ctx1 = new MyContext(db.Configure(new DbContextOptionsBuilder&lt;MyContext&gt;()).Options);
/// ctx1.Database.EnsureCreated();
/// // … write data …
/// ctx1.Dispose();
///
/// using var ctx2 = new MyContext(db.Configure(new DbContextOptionsBuilder&lt;MyContext&gt;()).Options);
/// // … data written by ctx1 is still here …
/// </code>
/// </summary>
public sealed class SqliteMemoryDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteMemoryDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    /// <summary>
    /// Configures the given <paramref name="builder"/> to use this in-memory
    /// database.  The schema is <b>not</b> automatically created; call
    /// <c>Database.EnsureCreated()</c> on the first context that uses this
    /// database.
    /// </summary>
    public DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder)
        => builder.UseSqlite(_connection);

    /// <summary>
    /// Generic overload of <see cref="Configure(DbContextOptionsBuilder)"/>.
    /// </summary>
    public DbContextOptionsBuilder<T> Configure<T>(DbContextOptionsBuilder<T> builder) where T : DbContext
        => (DbContextOptionsBuilder<T>)Configure((DbContextOptionsBuilder)builder);

    public void Dispose() => _connection.Dispose();
}
