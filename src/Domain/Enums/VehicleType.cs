using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VehicleType : byte
{
    Car = 1,
    Motorbike = 2,
    Emergency = 3,
    Bus = 4,
    Diplomat = 5,
    Military = 6,
    Foreign = 7,
}
