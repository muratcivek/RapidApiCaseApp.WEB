
using StayFinder.Web.Models.Booking;

namespace StayFinder.Web.Services.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyList<DestinationDto>> SearchDestinationsAsync(
        string query,
        CancellationToken cancellationToken = default);
}
