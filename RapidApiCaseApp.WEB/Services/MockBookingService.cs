
using System.Text.Json;
using Microsoft.Extensions.Options;
using StayFinder.Web.Configurations;
using StayFinder.Web.Models.Booking;
using StayFinder.Web.Services.Interfaces;

namespace StayFinder.Web.Services;

public sealed class MockBookingService : IBookingService
{
    private readonly IWebHostEnvironment _environment;
    private readonly BookingApiOptions _options;
    private readonly ILogger<MockBookingService> _logger;

    public MockBookingService(
        IWebHostEnvironment environment,
        IOptions<BookingApiOptions> options,
        ILogger<MockBookingService> logger)
    {
        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DestinationDto>>
        SearchDestinationsAsync(
            string query,
            CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(
            _environment.ContentRootPath,
            _options.MockDestinationFile);

        if (!File.Exists(filePath))
        {
            _logger.LogError(
                "Mock Booking dosyası bulunamadı: {FilePath}",
                filePath);

            throw new FileNotFoundException(
                "Mock Booking JSON dosyası bulunamadı.",
                filePath);
        }

        await using var stream = File.OpenRead(filePath);

        var response = await JsonSerializer
            .DeserializeAsync<DestinationResponse>(
                stream,
                cancellationToken: cancellationToken);

        if (response is null || !response.Status)
        {
            throw new InvalidOperationException(
                "Mock Booking JSON yanıtı geçersiz.");
        }

        _logger.LogInformation(
            "Mock Booking kullanıldı. Query: {Query}, Count: {Count}",
            query,
            response.Data.Count);

        return response.Data;
    }
}
