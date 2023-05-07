using System.ComponentModel.DataAnnotations.Schema;

namespace UsersManager.Infrastructure.Models;

[Table("user")]
public class DbUser
{
    public string Id { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedDate { get; set; }
    public DbUserGroup Group { get; set; }
    public DbUserState State { get; set; }
}