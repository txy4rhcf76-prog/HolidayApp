using HolidayApp.Api.Repositories;
using HolidayApp.Api.Services;
using HolidayApp.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HolidayApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        private readonly INagerClient _nagerClient;
        private readonly IHolidayRepository _repo;
        private readonly ILogger<HolidaysController> _logger;

        public HolidaysController(INagerClient nagerClient, IHolidayRepository repo, ILogger<HolidaysController> logger)
        {
            _nagerClient = nagerClient;
            _repo = repo;
            _logger = logger;
        }

        // Import holidays from external API and save to DB
        [HttpPost("import/{year}/{countryCode}")]
        public async Task<IActionResult> Import(int year, string countryCode, CancellationToken cancellationToken)
        {
            var holidays = await _nagerClient.GetPublicHolidaysAsync(year, countryCode, cancellationToken);
            var added = await _repo.SaveHolidaysAsync(holidays, cancellationToken);
            return Ok(new { Added = added, Retrieved = holidays.Count() });
        }

        // Get last celebrated n holidays for a country
        [HttpGet("last/{countryCode}")]
        public async Task<IActionResult> GetLast(string countryCode, [FromQuery] int count = 3, CancellationToken cancellationToken = default)
        {
            var holidays = await _repo.GetLastHolidaysAsync(countryCode, count, cancellationToken);
            var dto = holidays.Select(h => new { h.Date, h.LocalName });
            return Ok(dto);
        }

        // Get non-weekend holiday counts for multiple countries in a year
        [HttpGet("nonweekend-count")]
        public async Task<IActionResult> GetNonWeekendCounts([FromQuery] int year, [FromQuery] string countries)
        {
            var codes = (countries ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!codes.Any()) return BadRequest("Provide comma separated country codes in 'countries' query param.");

            var counts = await _repo.GetNonWeekendHolidayCountsAsync(year, codes);
            return Ok(counts.Select(c => new { c.CountryCode, c.Count }));
        }

        // Get deduplicated dates celebrated in both countries with local names
        [HttpGet("common")]
        public async Task<IActionResult> GetCommon([FromQuery] int year, [FromQuery] string c1, [FromQuery] string c2)
        {
            if (string.IsNullOrWhiteSpace(c1) || string.IsNullOrWhiteSpace(c2)) return BadRequest("Provide c1 and c2 country codes.");

            var common = await _repo.GetCommonHolidaysAsync(year, c1, c2);
            return Ok(common.Select(x => new { Date = x.Date, LocalName1 = x.LocalName1, LocalName2 = x.LocalName2 }));
        }
    }
}
