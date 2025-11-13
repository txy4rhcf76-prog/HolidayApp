using HolidayApp.Api.Models;

namespace HolidayApp.Api.Services
{
    public interface INagerClient
    {
        Task<IEnumerable<Holiday>> GetPublicHolidaysAsync(int year, string countryCode, CancellationToken cancellationToken = default);
    }
}
