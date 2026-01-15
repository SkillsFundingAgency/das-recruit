using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.ReferenceDataClientTests;

public class WhenGettingCandidateSkills
{
    [Test, MoqAutoData]
    public async Task Then_The_Skills_Are_Returned_And_Cached(
        List<string> expectedSkills,
        [Frozen] Mock<ICache> cache,
        [Greedy] ReferenceDataClient sut)
    {
        DateTime? capturedDateTime = null;
        cache
            .Setup(x => x.CacheAsideAsync(CacheKeys.Skills, It.IsAny<DateTime>(), It.IsAny<Func<Task<List<string>>>>()))
            .Callback<string, DateTime, Func<Task<List<string>>>>((_, dt, _) => { capturedDateTime = dt; })
            .ReturnsAsync(expectedSkills);

        // act
        var results = await sut.GetCandidateSkillsAsync();

        // assert
        results.Should().BeEquivalentTo(expectedSkills);
        capturedDateTime.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), TimeSpan.FromSeconds(1));
    }
}