using System;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.UnitTests.Mappers
{
    public class VacancyTraineeshipMapperTests
    {
        
        [Test, AutoData]
        public void Then_The_Request_Is_Mapped_To_The_TraineeshipVacancy(CreateTraineeshipVacancyRequest request, Guid id)
        {
            var actual = request.MapFromCreateTraineeshipVacancyRequest(id);

            actual.Id.Should().Be(id);
            actual.Should().BeEquivalentTo(request, options => options
                .Excluding(c => c.User)
                .Excluding(c => c.Address)
            );
            actual.EmployerLocation.Should().BeEquivalentTo(request.Address);
            actual.CreatedByUser.Should().BeEquivalentTo(request.User);
            actual.ApplicationMethod.Should().Be(ApplicationMethod.ThroughFindATraineeship);
        }
    }
}