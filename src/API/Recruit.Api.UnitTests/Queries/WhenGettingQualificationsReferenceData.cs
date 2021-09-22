﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries
{
    public class WhenGettingQualificationsReferenceData
    {
        [Test, MoqAutoData, Ignore("todo")]
        public async Task Then_The_Qualifications_Are_Returned(
            GetSkillsQuery query,//todo GetQualificationsQuery
            List<string> candidateQualifications,
            [Frozen] Mock<IRecruitVacancyClient> mockRecruitVacancyClient,
            GetSkillsQueryHandler handler)//todo GetQualificationsQueryHandler
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