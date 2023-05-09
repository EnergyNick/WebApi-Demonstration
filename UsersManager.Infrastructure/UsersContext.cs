using Microsoft.EntityFrameworkCore;
using UsersManager.Infrastructure.Models;

namespace UsersManager.Infrastructure;

public class UsersContext : DbContext
{
    public virtual DbSet<DbUser> Users => Set<DbUser>();
    public virtual DbSet<DbUserGroup> Groups => Set<DbUserGroup>();
    public virtual DbSet<DbUserState> States => Set<DbUserState>();

    public UsersContext()
    { }

    public UsersContext(DbContextOptions options) : base(options)
    { }
}