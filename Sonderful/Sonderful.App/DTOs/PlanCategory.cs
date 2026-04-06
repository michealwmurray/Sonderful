using System.Text.Json.Serialization;

namespace Sonderful.App.DTOs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlanCategory
{
    Walking,
    Coffee,
    Sports,
    Gaming,
    Dining,
    Other
}
