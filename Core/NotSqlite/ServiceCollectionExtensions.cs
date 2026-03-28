using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Not.Core.Persistence;

namespace Not.Sqlite;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a SQLite-backed <typeparamref name="TContext"/> and the NotFramework
    /// session infrastructure (<see cref="ISessionFactory"/>, <see cref="DatabaseContext"/>).
    /// </summary>
    public static IServiceCollection AddNotSQLite<TContext>(
        this IServiceCollection services,
        string connectionString)
        where TContext : DatabaseContext
    {
        services.AddDbContext<TContext>(opts => opts.UseSqlite(connectionString));
        services.AddScoped<DatabaseContext>(sp => sp.GetRequiredService<TContext>());
        services.AddSingleton<ISessionFactory, SessionFactory>();
        return services;
    }
}
