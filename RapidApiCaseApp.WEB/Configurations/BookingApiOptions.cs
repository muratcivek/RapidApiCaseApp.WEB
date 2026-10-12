
namespace StayFinder.Web.Configurations;

public sealed class BookingApiOptions
{
    public const string SectionName = "BookingApi";

    public bool UseMock { get; set; } = true;

    public string MockDestinationFile { get; set; } =
        string.Empty;
}
