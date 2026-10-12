
using System.Net.Http.Json;
using StayFinder.Web.Models.Booking;
using StayFinder.Web.Services.Interfaces;

namespace StayFinder.Web.Services;

public sealed class BookingService : IBookingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        HttpClient httpClient,
        ILogger<BookingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DestinationDto>>
        SearchDestinationsAsync(
            string query,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException(
                "Destinasyon boş olamaz.",
                nameof(query));
        }

        var encodedQuery = Uri.EscapeDataString(query.Trim());

        var endpoint =
            $"api/v1/hotels/searchDestination?query={encodedQuery}";

        using var response = await _httpClient.GetAsync(
            endpoint,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Booking API başarısız. StatusCode: {StatusCode}",
                (int)response.StatusCode);

            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content
            .ReadFromJsonAsync<DestinationResponse>(
                cancellationToken: cancellationToken);

        if (result is null || !result.Status)
        {
            _logger.LogWarning(
                "Booking API geçersiz yanıt döndürdü. Message: {Message}",
                result?.Message);

            throw new InvalidOperationException(
                "Destinasyon servisi geçerli bir yanıt döndürmedi.");
        }

        return result.Data;
    }
}
