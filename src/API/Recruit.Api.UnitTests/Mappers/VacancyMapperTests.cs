using System;
using System.Linq;
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
            // arrange
            request.Wage.WageType = WageType.NationalMinimumWageForApprentices;
            request.Wage.DurationUnit = DurationUnit.Month;
            request.ApplicationMethod = CreateVacancyApplicationMethod.ThroughExternalApplicationSite;
            request.AccountType = AccountType.Employer;
            
            // act
            var actual = request.MapFromCreateVacancyRequest(id);

            // assert
            actual.Id.Should().Be(id);
            actual.Should().BeEquivalentTo(request, options => options
                .Excluding(c => c.User)
                .Excluding(c => c.Address)
                .Excluding(c => c.Addresses)
                .Excluding(c => c.AccountType)
                .Excluding(c => c.Wage.DurationUnit)
            );
            
            actual.EmployerLocation.Should().BeEquivalentTo(request.Address);
            actual.CreatedByUser.Should().BeEquivalentTo(request.User);
            actual.OwnerType.Should().HaveSameValueAs(request.AccountType.Value);
            actual.Wage.DurationUnit.Should().Be(Esfa.Recruit.Vacancies.Client.Domain.Entities.DurationUnit.Month);
            actual.AdditionalQuestion1.Should().Be(request.AdditionalQuestion1);
            actual.AdditionalQuestion2.Should().Be(request.AdditionalQuestion2);
        }
        
        [Test, AutoData]
        public void Then_The_OneLocation_Request_Is_Mapped_To_The_Vacancy(CreateVacancyRequest request, Guid id)
        {
            // arrange
            request.EmployerLocationOption = AvailableWhere.OneLocation;
            request.Addresses = [request.Addresses.First()];
            request.Address = null;
            request.EmployerLocationInformation = null;
            
            // act
            var actual = request.MapFromCreateVacancyRequest(id);

            // assert
            actual.Id.Should().Be(id);
            actual.Should().BeEquivalentTo(request, options => options
                .Excluding(c => c.User)
                .Excluding(c => c.Wage.DurationUnit)
                .WithMapping<CreateVacancyRequest, Vacancy>(x => x.AccountType, x => x.OwnerType)
                .WithMapping<CreateVacancyRequest, Vacancy>(x => x.Address, x => x.EmployerLocation)
                .WithMapping<CreateVacancyRequest, Vacancy>(x => x.Addresses, x => x.EmployerLocations)
            );
        }
        
        [Test, AutoData]
        public void Then_The_MultipleLocations_Request_Is_Mapped_To_The_Vacancy(CreateVacancyRequest request, Guid id)
        {
            // arrange
            request.EmployerLocationOption = AvailableWhere.MultipleLocations;
            request.Address = null;
            request.EmployerLocationInformation = null;
            
            // act
            var actual = request.MapFromCreateVacancyRequest(id);

            // assert
            actual.Id.Should().Be(id);
            actual.Should().BeEquivalentTo(request, options => options
                .Excluding(c => c.User)
                .Excluding(c => c.Wage.DurationUnit)
                .WithMapping<CreateVacancyRequest, Vacancy>(x => x.AccountType, x => x.OwnerType)
                .WithMapping<CreateVacancyRequest, Vacancy>(x => x.Address, x => x.EmployerLocation)
                .WithMapping<CreateVacancyRequest, Vacancy>(x => x.Addresses, x => x.EmployerLocations)
            );
        }
        
        [Test, AutoData]
        public void Then_The_RecruitNationally_Request_Is_Mapped_To_The_Vacancy(CreateVacancyRequest request, Guid id)
        {
            // arrange
            request.EmployerLocationOption = AvailableWhere.AcrossEngland;
            request.Address = null;
            request.Addresses = null;
            
            // act
            var actual = request.MapFromCreateVacancyRequest(id);

            // assert
            actual.Id.Should().Be(id);
            actual.Should().BeEquivalentTo(request, options => options
                .Excluding(c => c.User)
                .Excluding(c => c.Wage.DurationUnit)
                .WithMapping<CreateVacancyRequest, Vacancy>(x => x.AccountType, x => x.OwnerType)
                .WithMapping<CreateVacancyRequest, Vacancy>(x => x.Address, x => x.EmployerLocation)
                .WithMapping<CreateVacancyRequest, Vacancy>(x => x.Addresses, x => x.EmployerLocations)
            );
        }
    }
}