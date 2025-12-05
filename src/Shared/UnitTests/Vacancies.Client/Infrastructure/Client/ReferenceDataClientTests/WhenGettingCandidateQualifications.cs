using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.ReferenceDataClientTests;

public class WhenGettingCandidateQualifications
{
    [Test, MoqAutoData]
    public async Task Then_The_Skills_Are_Returned_And_Cached(
        List<string> expectedQualifications,
        [Frozen] Mock<ICache> cache,
        [Greedy] ReferenceDataClient sut)
    {
        DateTime? capturedDateTime = null;
        cache
            .Setup(x => x.CacheAsideAsync(CacheKeys.Qualifications, It.IsAny<DateTime>(), It.IsAny<Func<Task<List<string>>>>()))
            .Callback<string, DateTime, Func<Task<List<string>>>>((_, dt, _) => { capturedDateTime = dt; })
            .ReturnsAsync(expectedQualifications);

        // act
        var results = await sut.GetCandidateQualificationsAsync();

        // assert
        results.Should().BeEquivalentTo(expectedQualifications);
        capturedDateTime.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), TimeSpan.FromSeconds(1));
    }
}