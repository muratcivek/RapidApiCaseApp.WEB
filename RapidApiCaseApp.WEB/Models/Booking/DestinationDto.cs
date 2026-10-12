
using System.Text.Json.Serialization;

namespace StayFinder.Web.Models.Booking;

public sealed class DestinationDto
{
    [JsonPropertyName("dest_id")]
    public string DestId { get; set; } = string.Empty;

    [JsonPropertyName("search_type")]
    public string SearchType { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("city_name")]
    public string? CityName { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("nr_hotels")]
    public int? HotelCount { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("cc1")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("dest_type")]
    public string? DestinationType { get; set; }
}
