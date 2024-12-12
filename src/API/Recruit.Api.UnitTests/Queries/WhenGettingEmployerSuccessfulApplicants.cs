using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Recruit.Api.Services;
using SFA.DAS.Testing.AutoFixture;
using ApplicationReviewStatus = SFA.DAS.Recruit.Api.Services.ApplicationReviewStatus;
using VacancyStatus = Esfa.Recruit.Vacancies.Client.Domain.Entities.VacancyStatus;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries;

public class WhenGettingEmployerSuccessfulApplicants
{
    private const string ValidEmployerAccountId = "AAA1A1";

    [Test, MoqAutoData]
    public async Task Then_If_EmployerAccountId_Is_Not_The_Correct_Format_Then_Return_InvalidRequest(
        Mock<IVacancyQuery> mockVacancyQuery,
        GetEmployerSuccessfulApplicantsQueryHandler sut)
    {
        var result = await sut.Handle(new GetEmployerSuccessfulApplicantsQuery("11111"), CancellationToken.None);
        result.ResultCode.Should().Be(ResponseCode.InvalidRequest);

        mockVacancyQuery.Verify(x => x.GetVacanciesByEmployerAccountAsync<VacancyIdentifier>(It.IsAny<string>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Then_If_EmployerAccountId_Is_Empty_Then_Return_InvalidRequest(
        Mock<IVacancyQuery> mockVacancyQuery,
        GetEmployerSuccessfulApplicantsQueryHandler sut)
    {
        var result = await sut.Handle(new GetEmployerSuccessfulApplicantsQuery(string.Empty), CancellationToken.None);
        result.ResultCode.Should().Be(ResponseCode.InvalidRequest);

        mockVacancyQuery.Verify(x => x.GetVacanciesByEmployerAccountAsync<VacancyIdentifier>(It.IsAny<string>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Then_If_No_Vacancies_Then_Return_Success_With_Empty_Data(
        GetEmployerSuccessfulApplicantsQuery request,
        [Frozen] Mock<IVacancyQuery> mockVacancyQuery,
        [Frozen] Mock<IQueryStoreReader> mockQueryStoreReader,
        GetEmployerSuccessfulApplicantsQueryHandler sut
    )
    {
        request.EmployerAccountId = ValidEmployerAccountId;

        mockVacancyQuery.Setup(x => x.GetVacanciesByEmployerAccountAsync<VacancyIdentifier>(request.EmployerAccountId))
            .ReturnsAsync(() => new List<VacancyIdentifier>());

        var result = await sut.Handle(request, CancellationToken.None);
        result.ResultCode.Should().Be(ResponseCode.Success);
        result.Data.Should().BeNull();

        mockVacancyQuery.Verify(x => x.GetVacanciesByEmployerAccountAsync<VacancyIdentifier>(request.EmployerAccountId), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Then_Applicants_Are_Returned_When_There_Are_Successful_Candidates(
        GetEmployerSuccessfulApplicantsQuery request,
        List<VacancyIdentifier> vacancies,
        [Frozen] Mock<IVacancyQuery> mockVacancyQuery,
        [Frozen] Mock<IQueryStoreReader> mockQueryStoreReader,
        GetEmployerSuccessfulApplicantsQueryHandler sut)
    {
        request.EmployerAccountId = ValidEmployerAccountId;

        var applications = new VacancyApplications();

        foreach (var vacancy in vacancies)
        {
            applications.Applications =
            [
                new() { CandidateId = Guid.NewGuid(), FirstName = "AAA", LastName = "AAA3", DateOfBirth = DateTime.Today, Status = ApplicationReviewStatus.Successful },
                new() { CandidateId = Guid.NewGuid(), FirstName = "AAA1", LastName = "AAA4", DateOfBirth = DateTime.Today, Status = ApplicationReviewStatus.Successful },
                new() { CandidateId = Guid.NewGuid(), FirstName = "AAA2", LastName = "AAA5", DateOfBirth = DateTime.Today, Status = ApplicationReviewStatus.Successful },
            ];
        }

        mockVacancyQuery.Setup(x => x.GetVacanciesByEmployerAccountAsync<VacancyIdentifier>(request.EmployerAccountId))
            .ReturnsAsync(vacancies);

        foreach (var vacancy in vacancies)
        {
            mockQueryStoreReader.Setup(x => x.GetVacancyApplicationsAsync(vacancy.VacancyReference.ToString()))
                .ReturnsAsync(applications);
        }

        var result = await sut.Handle(request, CancellationToken.None);
        result.ResultCode.Should().Be(ResponseCode.Success);
        result.Data.As<IOrderedEnumerable<SuccessfulApplicant>>().Count().Should().Be(applications.Applications.Count * vacancies.Count);

        mockVacancyQuery.Verify(x => x.GetVacanciesByEmployerAccountAsync<VacancyIdentifier>(request.EmployerAccountId), Times.Once);
        mockQueryStoreReader.Verify(x => x.GetVacancyApplicationsAsync(It.IsAny<string>()), Times.Exactly(vacancies.Count));
    }

    [Test, MoqAutoData]
    public async Task Then_Only_Successful_Applicants_Are_Returned(
        GetEmployerSuccessfulApplicantsQuery request,
        List<VacancyIdentifier> vacancies,
        [Frozen] Mock<IVacancyQuery> mockVacancyQuery,
        [Frozen] Mock<IQueryStoreReader> mockQueryStoreReader,
        GetEmployerSuccessfulApplicantsQueryHandler sut)
    {
        request.EmployerAccountId = ValidEmployerAccountId;

        var applications = new VacancyApplications();

        var successfulApplicantDateOfBirth = DateTime.Today.AddDays(-20);

        foreach (var vacancy in vacancies)
        {
            applications.Applications =
            [
                new() { CandidateId = Guid.NewGuid(), FirstName = "AAA", LastName = "AAA3", DateOfBirth = DateTime.Today, Status = ApplicationReviewStatus.New },
                new() { CandidateId = Guid.NewGuid(), FirstName = "AAA1", LastName = "AAA4", DateOfBirth = DateTime.Today, Status = ApplicationReviewStatus.Unsuccessful },
                new() { CandidateId = Guid.NewGuid(), FirstName = "Successful", LastName = "Applicant", DateOfBirth = successfulApplicantDateOfBirth, Status = ApplicationReviewStatus.Successful },
            ];
        }

        mockVacancyQuery.Setup(x => x.GetVacanciesByEmployerAccountAsync<VacancyIdentifier>(request.EmployerAccountId))
            .ReturnsAsync(vacancies);

        foreach (var vacancy in vacancies)
        {
            mockQueryStoreReader.Setup(x => x.GetVacancyApplicationsAsync(vacancy.VacancyReference.ToString()))
                .ReturnsAsync(applications);
        }

        var result = await sut.Handle(request, CancellationToken.None);
        result.ResultCode.Should().Be(ResponseCode.Success);
        var actualResult = result.Data.As<IOrderedEnumerable<SuccessfulApplicant>>();
        actualResult.Count().Should().Be(3);

        foreach (var applicant in actualResult)
        {
            applicant.FirstName.Should().Be("Successful");
            applicant.LastName.Should().Be("Applicant");
            applicant.DateOfBirth.Should().Be(successfulApplicantDateOfBirth);
        }

        mockVacancyQuery.Verify(x => x.GetVacanciesByEmployerAccountAsync<VacancyIdentifier>(request.EmployerAccountId), Times.Once);
        mockQueryStoreReader.Verify(x => x.GetVacancyApplicationsAsync(It.IsAny<string>()), Times.Exactly(vacancies.Count));
    }

    [Test, MoqAutoData]
    public async Task Then_Applicants_Are_Mapped_Correctly(
        GetEmployerSuccessfulApplicantsQuery request,
        [Frozen] Mock<IVacancyQuery> mockVacancyQuery,
        [Frozen] Mock<IQueryStoreReader> mockQueryStoreReader,
        GetEmployerSuccessfulApplicantsQueryHandler sut)
    {
        request.EmployerAccountId = ValidEmployerAccountId;

        List<VacancyIdentifier> vacancies = [new() { VacancyReference = 12323, Status = VacancyStatus.Approved, Id = Guid.NewGuid(), ClosingDate = DateTime.Today }];

        var applications = new VacancyApplications
        {
            Applications =
            [
                new() { CandidateId = Guid.NewGuid(), FirstName = "AAA", LastName = "AAA3", DateOfBirth = DateTime.Today, Status = ApplicationReviewStatus.Successful },
            ]
        };

        mockVacancyQuery.Setup(x => x.GetVacanciesByEmployerAccountAsync<VacancyIdentifier>(request.EmployerAccountId))
            .ReturnsAsync(vacancies);

        foreach (var vacancy in vacancies)
        {
            mockQueryStoreReader.Setup(x => x.GetVacancyApplicationsAsync(vacancy.VacancyReference.ToString()))
                .ReturnsAsync(applications);
        }

        var result = await sut.Handle(request, CancellationToken.None);
        result.ResultCode.Should().Be(ResponseCode.Success);

        var responseItem = result.Data.As<IOrderedEnumerable<SuccessfulApplicant>>().FirstOrDefault();
        responseItem.Should().NotBeNull();

        responseItem.ApplicantId.Should().Be(applications.Applications.First().CandidateId.ToString());
        responseItem.FirstName.Should().Be(applications.Applications.First().FirstName);
        responseItem.LastName.Should().Be(applications.Applications.First().LastName);
        responseItem.DateOfBirth.Should().Be(applications.Applications.First().DateOfBirth);
        responseItem.VacancyReference.Should().Be(vacancies.First().VacancyReference);
    }
}