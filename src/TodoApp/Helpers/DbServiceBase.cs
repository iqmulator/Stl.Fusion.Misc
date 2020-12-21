using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TodoApp.Helpers
{
    public abstract class DbServiceBase<TDbContext>
        where TDbContext : DbContext
    {
        protected IServiceProvider Services { get; }
        protected IDbContextFactory<TDbContext> DbContextFactory { get; }

        protected DbServiceBase(IServiceProvider services)
        {
            Services = services;
            DbContextFactory = services.GetRequiredService<IDbContextFactory<TDbContext>>();
        }

        protected TDbContext CreateDbContext()
            => DbContextFactory.CreateDbContext();
    }
}
