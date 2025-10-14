using System.Text.Json.Serialization;

namespace AuctionService.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    Live,
    Finished,
    ReserveNotMet

}
