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
    public class WhenGettingQualificationsReferenceData
    {
        [Test, MoqAutoData]
        public async Task Then_The_Qualifications_Are_Returned(
            GetQualificationsQuery query,
            List<string> candidateQualifications,
            [Frozen] Mock<IRecruitVacancyClient> mockRecruitVacancyClient,
            GetQualificationsQueryHandler handler) 
        {
            mockRecruitVacancyClient
                .Setup(x => x.GetCandidateQualificationsAsync())
                .ReturnsAsync(candidateQualifications);

            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Data.Should().BeEquivalentTo(candidateQualifications);
            actual.ResultCode.Should().Be(ResponseCode.Success);
        }
    }
}