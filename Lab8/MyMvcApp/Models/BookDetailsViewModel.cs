using System.Text.Json.Serialization;

namespace MyMvcApp.Models
{
    // 1. DTO for the OpenLibrary Search API Root JSON Response
    // Endpoint: https://openlibrary.org/search.json?title=...&limit=1
    public class OpenLibrarySearchResponse
    {
        [JsonPropertyName("numFound")]
        public int TotalMatches { get; set; }

        [JsonPropertyName("docs")]
        public List<OpenLibraryBookDto> Docs { get; set; } = new();
    }

    // 2. DTO for a single Book document inside OpenLibrary's "docs" array
    public class OpenLibraryBookDto
    {
        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("edition_count")]
        public int EditionCount { get; set; }

        [JsonPropertyName("cover_i")]
        public int? CoverImageId { get; set; }
    }

    // 3. DTO for the Currency Exchange API JSON Response
    // Endpoint: https://open.er-api.com/v6/latest/USD
    public class ExchangeRateApiResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = string.Empty;

        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; } = string.Empty;

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }

    // 4. Master ViewModel combining SQLite Book + OpenLibrary Metadata + INR Currency Conversion
    public class BookDetailsViewModel
    {
        // Local SQLite Entity
        public Book LocalBook { get; set; } = default!;

        // External API 1: OpenLibrary Metadata
        public OpenLibraryBookDto? ExternalMetadata { get; set; }

        // Helper property to build the cover image URL when CoverImageId is available
        public string? CoverImageUrl => ExternalMetadata?.CoverImageId != null
            ? $"https://covers.openlibrary.org/b/id/{ExternalMetadata.CoverImageId}-M.jpg"
            : null;

        // External API 2: Live USD to INR Currency Conversion
        public decimal? UsdToInrRate { get; set; }
        public decimal? PriceInInr => UsdToInrRate.HasValue
            ? Math.Round(LocalBook.Price * UsdToInrRate.Value, 2)
            : null;

        // Graceful Degradation warning message if either external API is unreachable
        public List<string> ApiWarnings { get; set; } = new();
    }
}