using System.Net.Http.Json;
using HolidayApp.Api.Models;

namespace HolidayApp.Api.Services
{
    public class NagerClient : INagerClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<NagerClient> _logger;

        public NagerClient(HttpClient client, ILogger<NagerClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<IEnumerable<Holiday>> GetPublicHolidaysAsync(int year, string countryCode, CancellationToken cancellationToken = default)
        {
            var url = $"PublicHolidays/{year}/{countryCode}";
            try
            {
                var response = await _client.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var apiHolidays = await response.Content.ReadFromJsonAsync<IEnumerable<ApiHoliday>>(cancellationToken: cancellationToken);
                if (apiHolidays == null) return Enumerable.Empty<Holiday>();

                // Map to local Holiday entity
                return apiHolidays.Select(a => new Holiday
                {
                    CountryCode = countryCode,
                    Date = a.Date,
                    LocalName = a.LocalName,
                    Name = a.Name,
                    Global = a.Global,
                    Counties = a.Counties is null ? null : string.Join(',', a.Counties),
                    LaunchYear = a.LaunchYear,
                    Types = a.Types is null ? null : string.Join(',', a.Types)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch holidays from Nager API for {year}/{country}", year, countryCode);
                throw;
            }
        }

        private class ApiHoliday
        {
            public DateTime Date { get; set; }
            public string LocalName { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string?[]? Counties { get; set; }
            public bool Global { get; set; }
            public int? LaunchYear { get; set; }
            public string?[]? Types { get; set; }
        }
    }
}
