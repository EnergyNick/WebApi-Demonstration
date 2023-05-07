using System.Text.Json.Serialization;

namespace UsersManager.Domain.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GroupCode
{
    User,
    Admin
}