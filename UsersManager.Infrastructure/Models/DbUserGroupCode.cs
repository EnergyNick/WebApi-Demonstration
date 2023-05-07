using System.Text.Json.Serialization;

namespace UsersManager.Infrastructure.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DbUserGroupCode
{
    User,
    Admin
}