using System.Text.Json;
using static LibraryManagementSystem.DTOs.Dtos;

namespace LibraryManagementSystem.Services
{
    public interface IExternalBookService
    {
        Task<ExternalBookDetail> FetchBookDetailAsync(string isbn, CancellationToken ct = default);
    }
    public class ExternalBookService : IExternalBookService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ExternalBookService> _logger;

        // Open Library Work endpoint: https://openlibrary.org/isbn/{isbn}.json
        private const string BaseUrl = "https://openlibrary.org/isbn/";

        public ExternalBookService(HttpClient http, ILogger<ExternalBookService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<ExternalBookDetail> FetchBookDetailAsync(
            string isbn, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Fetching external details for ISBN {ISBN}", isbn);

                var url = $"{BaseUrl}{isbn}.json";
                using var response = await _http.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Open Library returned {Code} for ISBN {ISBN}",
                                       response.StatusCode, isbn);
                    return BuildMockDetail(isbn);
                }

                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Open Library fields vary — extract what's available
                var title = root.TryGetProperty("title", out var t) ? t.GetString() : "N/A";
                var publisher = root.TryGetProperty("publishers", out var p) && p.GetArrayLength() > 0
                                ? p[0].GetString() : "Unknown";
                var pubDate = root.TryGetProperty("publish_date", out var d) ? d.GetString() : "Unknown";
                var desc = root.TryGetProperty("description", out var de)
                                ? (de.ValueKind == JsonValueKind.Object
                                   ? de.GetProperty("value").GetString()
                                   : de.GetString())
                                : "No description available.";

                var coverUrl = root.TryGetProperty("covers", out var c) && c.GetArrayLength() > 0
                                ? $"https://covers.openlibrary.org/b/id/{c[0].GetInt32()}-L.jpg"
                                : string.Empty;

                // Author names live in /authors/{key}.json — simplified here
                var author = root.TryGetProperty("by_statement", out var a) ? a.GetString() : "See Open Library";

                return new ExternalBookDetail(
                    Title: title ?? "N/A",
                    Author: author ?? "N/A",
                    Publisher: publisher ?? "N/A",
                    PublishedDate: pubDate ?? "N/A",
                    Description: desc ?? "N/A",
                    ThumbnailUrl: coverUrl,
                    Source: "Open Library API (openlibrary.org)"
                );
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "External API call failed for ISBN {ISBN} — using mock data", isbn);
                return BuildMockDetail(isbn);
            }
        }

        // ── Fallback mock (used when the external API is unavailable) ─────────────
        private static ExternalBookDetail BuildMockDetail(string isbn) =>
            new(
                Title: $"Book with ISBN {isbn}",
                Author: "Author Unavailable",
                Publisher: "Publisher Unavailable",
                PublishedDate: "N/A",
                Description: "External API was unreachable. This is a simulated fallback response " +
                               "demonstrating async/await error-handling patterns.",
                ThumbnailUrl: "https://via.placeholder.com/128x192?text=No+Cover",
                Source: "Mock Fallback (Open Library API unreachable)"
            );
    }
}
