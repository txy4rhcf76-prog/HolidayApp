using HolidayApp.Api.Data;
using HolidayApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HolidayApp.Api.Repositories
{
    public class HolidayRepository : IHolidayRepository
    {
        private readonly AppDbContext _db;

        public HolidayRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<int> SaveHolidaysAsync(IEnumerable<Holiday> holidays, CancellationToken cancellationToken = default)
        {
            // More efficient and safe insert: fetch existing keys first to avoid tracking conflicts
            var list = holidays.ToList();
            if (!list.Any()) return 0;

            // Build a simple string key to compare existing rows
            var countryCodes = list.Select(x => x.CountryCode).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            var minDate = list.Min(x => x.Date);
            var maxDate = list.Max(x => x.Date);

            var existingKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            var existing = await _db.Holidays.AsNoTracking()
                .Where(h => countryCodes.Contains(h.CountryCode) && h.Date >= minDate && h.Date <= maxDate)
                .Select(h => new { h.CountryCode, h.Date, h.LocalName })
                .ToListAsync(cancellationToken);

            foreach (var e in existing)
            {
                existingKeys.Add($"{e.CountryCode}|{e.Date:o}|{e.LocalName}");
            }

            int added = 0;
            foreach (var h in list)
            {
                var key = $"{h.CountryCode}|{h.Date:o}|{h.LocalName}";
                if (!existingKeys.Contains(key))
                {
                    _db.Holidays.Add(h);
                    existingKeys.Add(key);
                    added++;
                }
            }

            if (added > 0) await _db.SaveChangesAsync(cancellationToken);
            return added;
        }

        public async Task<IEnumerable<Holiday>> GetLastHolidaysAsync(string countryCode, int count, CancellationToken cancellationToken = default)
        {
            return await _db.Holidays
                .AsNoTracking()
                .Where(h => h.CountryCode == countryCode)
                .OrderByDescending(h => h.Date)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<(string CountryCode, int Count)>> GetNonWeekendHolidayCountsAsync(int year, IEnumerable<string> countryCodes, CancellationToken cancellationToken = default)
        {
            var codes = countryCodes.Select(c => c.ToUpperInvariant()).ToList();

            // Some DateTime members (e.g. DayOfWeek) may not translate to SQL for all providers/EF versions.
            // Fetch the candidate rows for the year and selected countries, then apply weekend filter in memory.
            var rows = await _db.Holidays.AsNoTracking()
                .Where(h => h.Date.Year == year && codes.Contains(h.CountryCode.ToUpper()))
                .ToListAsync(cancellationToken);

            var grouped = rows
                .GroupBy(h => h.CountryCode)
                .Select(g => new { Country = g.Key, Count = g.Select(x => x.Date).Where(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday).Distinct().Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            return grouped.Select(x => (x.Country, x.Count));
        }

        public async Task<IEnumerable<(DateTime Date, string LocalName1, string LocalName2)>> GetCommonHolidaysAsync(int year, string countryCode1, string countryCode2, CancellationToken cancellationToken = default)
        {
            var c1 = countryCode1.ToUpperInvariant();
            var c2 = countryCode2.ToUpperInvariant();

            var h1 = _db.Holidays.AsNoTracking()
                .Where(h => h.CountryCode.ToUpper() == c1 && h.Date.Year == year)
                .Select(h => new { h.Date, h.LocalName });

            var h2 = _db.Holidays.AsNoTracking()
                .Where(h => h.CountryCode.ToUpper() == c2 && h.Date.Year == year)
                .Select(h => new { h.Date, h.LocalName });

            var join = from a in h1
                       join b in h2 on a.Date equals b.Date
                       select new { a.Date, LocalName1 = a.LocalName, LocalName2 = b.LocalName };

            var result = await join.Distinct().ToListAsync(cancellationToken);
            return result.Select(x => (x.Date, x.LocalName1, x.LocalName2));
        }
    }
}
