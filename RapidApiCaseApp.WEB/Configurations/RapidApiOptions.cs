
using System.ComponentModel.DataAnnotations;

namespace StayFinder.Web.Options;

public sealed class RapidApiOptions
{
    public const string SectionName = "RapidApi";

    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Host { get; set; } = string.Empty;

    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;
}
