using Microsoft.EntityFrameworkCore;
using UsersManager.Infrastructure.Models;

namespace UsersManager.Infrastructure;

public class UsersContext : DbContext
{
    public DbSet<DbUser> Users => Set<DbUser>();
    public DbSet<DbUserGroup> Groups => Set<DbUserGroup>();
    public DbSet<DbUserState> States => Set<DbUserState>();

    public UsersContext(DbContextOptions options) : base(options)
    {
        base.Database.EnsureCreated();
    }
}