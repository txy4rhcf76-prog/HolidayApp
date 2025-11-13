using HolidayApp.Api.Models;

namespace HolidayApp.Api.Repositories
{
    public interface IHolidayRepository
    {
        Task<int> SaveHolidaysAsync(IEnumerable<Holiday> holidays, CancellationToken cancellationToken = default);

        Task<IEnumerable<Holiday>> GetLastHolidaysAsync(string countryCode, int count, CancellationToken cancellationToken = default);

        Task<IEnumerable<(string CountryCode, int Count)>> GetNonWeekendHolidayCountsAsync(int year, IEnumerable<string> countryCodes, CancellationToken cancellationToken = default);

        Task<IEnumerable<(DateTime Date, string LocalName1, string LocalName2)>> GetCommonHolidaysAsync(int year, string countryCode1, string countryCode2, CancellationToken cancellationToken = default);
    }
}
