using System.ComponentModel.DataAnnotations.Schema;

namespace UsersManager.Infrastructure.Models;

[Table("user_group")]
public class DbUserGroup
{
    public string Id { get; set; }
    public DbUserGroupCode Code { get; set; }
    public string? Description { get; set; }
}