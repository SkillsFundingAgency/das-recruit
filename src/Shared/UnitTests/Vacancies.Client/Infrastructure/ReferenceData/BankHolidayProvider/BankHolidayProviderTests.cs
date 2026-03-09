using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.ReferenceData.BankHolidayProvider;

public class BankHolidayProviderTests
{
    [Test, MoqAutoData]
    public async Task Then_The_BankHolidays_Are_Retrieved_From_The_Cache(
        BankHolidaysData apiResponse,
        [Frozen] Mock<ICache> cache,
        Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays.BankHolidayProvider provider)
    {
        apiResponse.EnglandAndWales.Events =
            [new Event { Date = new DateTime(2025, 8, 31).ToString("yyyy-MM-dd") }];
        cache.Setup(x => x.CacheAsideAsync(CacheKeys.BankHolidays, It.IsAny<DateTime>(),
                It.IsAny<Func<Task<List<DateTime>>>>() )).ReturnsAsync(apiResponse.EnglandAndWales.Events.Select(c=>DateTime.Parse(c.Date)).ToList);
        
        var actual = await provider.GetBankHolidaysAsync();

        actual.Should().BeEquivalentTo(apiResponse.EnglandAndWales.Events.Select(c=>DateTime.Parse(c.Date)));
    }
}