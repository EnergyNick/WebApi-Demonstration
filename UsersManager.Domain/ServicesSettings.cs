using System.ComponentModel.DataAnnotations;

namespace UsersManager.Domain;

public class ServicesSettings
{
    public static readonly string SectionName = "Services";

    [Required]
    public TimeSpan UserCreationTimeout { get; init; }
}