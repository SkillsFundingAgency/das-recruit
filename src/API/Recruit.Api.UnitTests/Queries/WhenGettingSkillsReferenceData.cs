using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries
{
    public class WhenGettingSkillsReferenceData
    {
        [Test, MoqAutoData]
        public async Task Then_The_Skills_Are_Returned(
            GetSkillsQuery query,
            List<string> candidateSkills,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            GetSkillsQueryHandler handler)
        {
            recruitVacancyClient.Setup(x => x.GetCandidateSkillsAsync()).ReturnsAsync(candidateSkills);

            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Data.Should().BeEquivalentTo(candidateSkills);
            actual.ResultCode.Should().Be(ResponseCode.Success);
        }
    }
}