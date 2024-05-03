using System;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class WhenConstructingPostApplicationStatusRequest
{
    [Test, AutoData]
    public void Then_It_Is_Correctly_Constructed(
        Guid candidateId, 
        Guid applicationId,
        PostApplicationStatus postApplicationStatus)
    {
        var actual = new PostApplicationStatusRequest(candidateId, applicationId, postApplicationStatus);

        actual.PostUrl.Should().Be($"candidates/{candidateId}/applications/{applicationId}");
        ((PostApplicationStatus)actual.Data).Should().BeEquivalentTo(postApplicationStatus);
    }
}