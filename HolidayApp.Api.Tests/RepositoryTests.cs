using System;
using System.Linq;
using System.Threading.Tasks;
using HolidayApp.Api.Data;
using HolidayApp.Api.Models;
using HolidayApp.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HolidayApp.Api.Tests
{
    public class RepositoryTests
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetLastHolidaysAsync_Returns_Last_N()
        {
            using var db = CreateContext("last_test");
            db.Holidays.AddRange(
                new Holiday { CountryCode = "GB", Date = new DateTime(2024,12,25), LocalName = "Christmas Day" },
                new Holiday { CountryCode = "GB", Date = new DateTime(2024,12,26), LocalName = "Boxing Day" },
                new Holiday { CountryCode = "GB", Date = new DateTime(2024,1,1), LocalName = "New Year" }
            );
            await db.SaveChangesAsync();

            var repo = new HolidayRepository(db);
            var last = (await repo.GetLastHolidaysAsync("GB", 2)).ToList();

            Assert.Equal(2, last.Count);
            Assert.Equal(new DateTime(2024,12,26), last[0].Date);
            Assert.Equal("Boxing Day", last[0].LocalName);
        }

        [Fact]
        public async Task GetNonWeekendHolidayCountsAsync_Ignores_Weekends()
        {
            using var db = CreateContext("count_test");
            // 2024-01-01 is Monday, 2024-01-06 is Saturday
            db.Holidays.AddRange(
                new Holiday { CountryCode = "US", Date = new DateTime(2024,1,1), LocalName = "New Year" },
                new Holiday { CountryCode = "US", Date = new DateTime(2024,1,6), LocalName = "WeekendHoliday" },
                new Holiday { CountryCode = "GB", Date = new DateTime(2024,1,1), LocalName = "New Year" }
            );
            await db.SaveChangesAsync();

            var repo = new HolidayRepository(db);
            var counts = (await repo.GetNonWeekendHolidayCountsAsync(2024, new[] { "US", "GB" })).ToList();

            var us = counts.First(c => c.CountryCode == "US");
            var gb = counts.First(c => c.CountryCode == "GB");

            Assert.Equal(1, us.Count); // weekend holiday excluded
            Assert.Equal(1, gb.Count);
        }

        [Fact]
        public async Task GetCommonHolidaysAsync_Returns_Matching_Dates()
        {
            using var db = CreateContext("common_test");
            db.Holidays.AddRange(
                new Holiday { CountryCode = "GB", Date = new DateTime(2024,12,25), LocalName = "Christmas GB" },
                new Holiday { CountryCode = "US", Date = new DateTime(2024,12,25), LocalName = "Christmas US" },
                new Holiday { CountryCode = "GB", Date = new DateTime(2024,11,5), LocalName = "Guy Fawkes" }
            );
            await db.SaveChangesAsync();

            var repo = new HolidayRepository(db);
            var common = (await repo.GetCommonHolidaysAsync(2024, "GB", "US")).ToList();

            Assert.Single(common);
            Assert.Equal(new DateTime(2024,12,25), common[0].Date);
        }
    }
}
