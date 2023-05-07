using System.ComponentModel.DataAnnotations;

namespace UsersManager.Infrastructure;

public record DataBaseSettings
{
    public static readonly string SectionName = "DataBase";

    [Required]
    public string ConnectionString { get; init; }
}