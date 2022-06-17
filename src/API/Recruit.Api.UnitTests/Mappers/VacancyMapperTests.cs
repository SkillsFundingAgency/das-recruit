using System;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Models;
using DurationUnit = SFA.DAS.Recruit.Api.Models.DurationUnit;
using WageType = SFA.DAS.Recruit.Api.Models.WageType;

namespace SFA.DAS.Recruit.Api.UnitTests.Mappers
{
    public class VacancyMapperTests
    {
        [Test, AutoData]
        public void Then_The_Request_Is_Mapped_To_The_Vacancy(CreateVacancyRequest request, Guid id)
        {
            request.Wage.WageType = WageType.NationalMinimumWageForApprentices;
            request.Wage.DurationUnit = DurationUnit.Year;
            request.ApplicationMethod = CreateVacancyApplicationMethod.ThroughExternalApplicationSite;
            request.AccountType = AccountType.Employer;
            
            var actual = request.MapFromCreateVacancyRequest(id);

            actual.Id.Should().Be(id);
            actual.Should().BeEquivalentTo(request, options => options
                .Excluding(c => c.User)
                .Excluding(c => c.Address)
                .Excluding(c=> c.AccountType)
            );
            actual.EmployerLocation.Should().BeEquivalentTo(request.Address);
            actual.CreatedByUser.Should().BeEquivalentTo(request.User);
            actual.OwnerType.Should().HaveSameValueAs(request.AccountType);
        }
    }
}