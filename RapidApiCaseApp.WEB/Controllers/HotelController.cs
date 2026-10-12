
using Microsoft.AspNetCore.Mvc;
using StayFinder.Web.Models.Booking;
using StayFinder.Web.Services.Interfaces;

namespace StayFinder.Web.Controllers;

public sealed class HotelController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<HotelController> _logger;

    public HotelController(
        IBookingService bookingService,
        ILogger<HotelController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult HotelList(
        [FromQuery] HotelSearchRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .SelectMany(x =>
                    x.Value?.Errors
                    ?? Enumerable.Empty<
                        Microsoft.AspNetCore.Mvc.ModelBinding.ModelError>())
                .Select(x => x.ErrorMessage)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            TempData["Error"] = errors.Length > 0
                ? string.Join(" ", errors)
                : "Arama bilgileri geçersiz.";

            return RedirectToAction("Index", "Home");
        }

        _logger.LogInformation(
            "Otel arama isteği. Destination: {Destination}, " +
            "DestId: {DestId}, SearchType: {SearchType}, " +
            "Checkin: {Checkin}, Checkout: {Checkout}",
            request.Destination,
            request.DestId,
            request.SearchType,
            request.Checkin,
            request.Checkout);

        // Destinasyon autocomplete sırasında seçildi.
        // Bu nedenle burada SearchDestinationsAsync çağrılmaz.
        //
        // Sonraki aşamada Search Hotels endpoint'i
        // request.DestId ve request.SearchType ile kullanılacak.

        return View("HotelList", request);
    }

    [HttpGet]
    public async Task<IActionResult> SearchDestinations(
        [FromQuery] string? query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) ||
            query.Trim().Length < 2)
        {
            return Json(Array.Empty<object>());
        }

        try
        {
            var destinations = await _bookingService
                .SearchDestinationsAsync(
                    query.Trim(),
                    cancellationToken);

            var result = destinations
                .Select(x => new
                {
                    destId = x.DestId,
                    searchType = x.SearchType,
                    name = x.Name,
                    label = x.Label,
                    country = x.Country,
                    region = x.Region,
                    imageUrl = x.ImageUrl,
                    hotelCount = x.HotelCount
                })
                .ToList();

            return Json(result);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Destinasyon API HTTP hatası. Query: {Query}",
                query);

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message = "Destinasyon servisine ulaşılamadı."
                });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(
                ex,
                "Destinasyon API zaman aşımı. Query: {Query}",
                query);

            return StatusCode(
                StatusCodes.Status504GatewayTimeout,
                new
                {
                    message = "Destinasyon servisi zaman aşımına uğradı."
                });
        }
        catch (IOException ex)
        {
            _logger.LogError(
                ex,
                "Mock destinasyon dosyası okunamadı.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "Destinasyon verisi okunamadı."
                });
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(
                ex,
                "Destinasyon JSON verisi geçersiz.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "Destinasyon verisi geçersiz."
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Destinasyon servis yanıtı geçersiz.");

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message = "Destinasyon servisinden geçerli yanıt alınamadı."
                });
        }
    }


    [HttpGet]
    public IActionResult ValidateSearch(
        [FromQuery] HotelSearchRequest request)
    {
        if (ModelState.IsValid)
        {
            return Ok(new
            {
                valid = true,
                errors = Array.Empty<object>()
            });
        }

        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(error => new
            {
                field = x.Key,
                message = string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Geçersiz değer."
                    : error.ErrorMessage
            }))
            .DistinctBy(x => new { x.field, x.message })
            .ToList();

        return BadRequest(new
        {
            valid = false,
            errors
        });
    }

}
