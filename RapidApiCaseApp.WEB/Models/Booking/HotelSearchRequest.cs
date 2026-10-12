
using System.ComponentModel.DataAnnotations;

namespace StayFinder.Web.Models.Booking;

public sealed class HotelSearchRequest : IValidatableObject
{
    [Required(ErrorMessage = "Destinasyon zorunludur.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Destinasyon 2-100 karakter olmalıdır.")]
    public string Destination { get; set; } = string.Empty;

    [Required(ErrorMessage = "Giriş tarihi zorunludur.")]
    public DateOnly? Checkin { get; set; }

    [Required(ErrorMessage = "Çıkış tarihi zorunludur.")]
    public DateOnly? Checkout { get; set; }

    [Required]
    public string Currency { get; set; } = "EUR";

    [Range(1, 10, ErrorMessage = "Yetişkin sayısı 1-10 olmalıdır.")]
    public int Adults { get; set; } = 2;

    [Range(0, 6, ErrorMessage = "Çocuk sayısı 0-6 olmalıdır.")]
    public int Children { get; set; }

    [Range(1, 5, ErrorMessage = "Oda sayısı 1-5 olmalıdır.")]
    public int Rooms { get; set; } = 1;

    public List<int> ChildAges { get; set; } = new();

    [Required(ErrorMessage = "Lütfen listeden bir destinasyon seçin.")]
    public string DestId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Destinasyon türü bulunamadı.")]
    public string SearchType { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Checkin.HasValue &&
            Checkin.Value < DateOnly.FromDateTime(DateTime.Today))
        {
            yield return new ValidationResult(
                "Giriş tarihi geçmiş bir tarih olamaz.",
                new[] { nameof(Checkin) });
        }

        if (Checkin.HasValue && Checkout.HasValue &&
            Checkout.Value <= Checkin.Value)
        {
            yield return new ValidationResult(
                "Çıkış tarihi giriş tarihinden sonra olmalıdır.",
                new[] { nameof(Checkout) });
        }

        if (!new[] { "EUR", "USD", "TRY", "GBP" }
            .Contains(Currency, StringComparer.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Geçersiz para birimi.",
                new[] { nameof(Currency) });
        }

        if (ChildAges.Count != Children)
        {
            yield return new ValidationResult(
                "Her çocuk için bir yaş seçilmelidir.",
                new[] { nameof(ChildAges) });
        }

        if (ChildAges.Any(age => age < 0 || age > 17))
        {
            yield return new ValidationResult(
                "Çocuk yaşları 0-17 arasında olmalıdır.",
                new[] { nameof(ChildAges) });
        }
    }
}
