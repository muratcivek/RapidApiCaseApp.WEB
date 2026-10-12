
using System.Text.Json.Serialization;

namespace StayFinder.Web.Models.Booking;

public sealed class DestinationResponse
{
    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public List<DestinationDto> Data { get; set; } = new();
}
