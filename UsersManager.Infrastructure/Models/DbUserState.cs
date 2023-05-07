using System.ComponentModel.DataAnnotations.Schema;

namespace UsersManager.Infrastructure.Models;

[Table("user_state")]
public class DbUserState
{
    public string Id { get; set; }
    public DbUserStateCode Code { get; set; }
    public string? Description { get; set; }
}