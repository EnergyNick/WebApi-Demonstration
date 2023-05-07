using System.Text.Json.Serialization;

namespace UsersManager.Domain.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StateCode
{
    Created,
    Active,
    Blocked
}