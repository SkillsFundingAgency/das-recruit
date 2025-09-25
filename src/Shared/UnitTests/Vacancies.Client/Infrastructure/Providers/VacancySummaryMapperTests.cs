using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Providers;

public class VacancySummaryMapperTests
{
    [Fact]
    public void Then_HasSubmittedAdditionalQuestion_True_MapFromVacancySummaryAggQueryResponseDto_ShouldMapCorrectly()
    {
        // Arrange
        var v = new Fixture().Create<VacancySummaryAggQueryResponseDto>();
        v.Id.OwnerType = OwnerType.Provider;
        v.Id.HasSubmittedAdditionalQuestions = true;

        // Act
        var result = VacancySummaryMapper.MapFromVacancySummaryAggQueryResponseDto(v);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(v.Id.VacancyGuid);
        result.Title.Should().Be(v.Id.Title);
        result.VacancyReference.Should().Be(v.Id.VacancyReference);
        result.LegalEntityName.Should().Be(v.Id.LegalEntityName);
        result.EmployerAccountId.Should().Be(v.Id.EmployerAccountId);
        result.EmployerName.Should().Be(v.Id.EmployerName);
        result.Ukprn.Should().Be(v.Id.Ukprn);
        result.CreatedDate.Should().Be(v.Id.CreatedDate);
        result.Duration.Should().Be(v.Id.Duration);
        result.DurationUnit.Should().Be(v.Id.DurationUnit);
        result.Status.Should().Be(v.Id.Status);
        result.ClosingDate.Should().Be(v.Id.ClosingDate);
        result.ClosedDate.Should().Be(v.Id.ClosedDate);
        result.ClosureReason.Should().Be(v.Id.ClosureReason);
        result.ApplicationMethod.Should().Be(v.Id.ApplicationMethod);
        result.ProgrammeId.Should().Be(v.Id.ProgrammeId);
        result.StartDate.Should().Be(v.Id.StartDate);
        result.TrainingTitle.Should().Be(v.Id.TrainingTitle);
        result.TrainingType.Should().Be(v.Id.TrainingType);
        result.TrainingLevel.Should().Be(v.Id.TrainingLevel);
        result.TransferInfoUkprn.Should().Be(v.Id.TransferInfoUkprn);
        result.TransferInfoProviderName.Should().Be(v.Id.TransferInfoProviderName);
        result.TransferInfoReason.Should().Be(v.Id.TransferInfoReason);
        result.TransferInfoTransferredDate.Should().Be(v.Id.TransferInfoTransferredDate);
        result.TrainingProviderName.Should().Be(v.Id.TrainingProviderName);
        result.NoOfNewApplications.Should().Be(v.NoOfNewApplications);
        result.NoOfSuccessfulApplications.Should().Be(v.NoOfSuccessfulApplications);
        result.NoOfUnsuccessfulApplications.Should().Be(v.NoOfUnsuccessfulApplications);
        result.NoOfSharedApplications.Should().Be(v.NoOfSharedApplications);
        result.NoOfAllSharedApplications.Should().Be(v.NoOfAllSharedApplications);
        result.NoOfEmployerReviewedApplications.Should().Be(v.NoOfEmployerReviewedApplications);
        result.IsTraineeship.Should().Be(v.Id.IsTraineeship);
        result.IsTaskListCompleted.Should().BeTrue();
        result.HasChosenProviderContactDetails.Should().Be(v.Id.HasChosenProviderContactDetails);
    }

    [Fact]
    public void Then_HasSubmittedAdditionalQuestion_False_MapFromVacancySummaryAggQueryResponseDto_ShouldMapCorrectly()
    {
        // Arrange
        var v = new Fixture().Create<VacancySummaryAggQueryResponseDto>();
        v.Id.OwnerType = OwnerType.Provider;
        v.Id.HasSubmittedAdditionalQuestions = false;

        // Act
        var result = VacancySummaryMapper.MapFromVacancySummaryAggQueryResponseDto(v);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(v.Id.VacancyGuid);
        result.Title.Should().Be(v.Id.Title);
        result.VacancyReference.Should().Be(v.Id.VacancyReference);
        result.LegalEntityName.Should().Be(v.Id.LegalEntityName);
        result.EmployerAccountId.Should().Be(v.Id.EmployerAccountId);
        result.EmployerName.Should().Be(v.Id.EmployerName);
        result.Ukprn.Should().Be(v.Id.Ukprn);
        result.CreatedDate.Should().Be(v.Id.CreatedDate);
        result.Duration.Should().Be(v.Id.Duration);
        result.DurationUnit.Should().Be(v.Id.DurationUnit);
        result.Status.Should().Be(v.Id.Status);
        result.ClosingDate.Should().Be(v.Id.ClosingDate);
        result.ClosedDate.Should().Be(v.Id.ClosedDate);
        result.ClosureReason.Should().Be(v.Id.ClosureReason);
        result.ApplicationMethod.Should().Be(v.Id.ApplicationMethod);
        result.ProgrammeId.Should().Be(v.Id.ProgrammeId);
        result.StartDate.Should().Be(v.Id.StartDate);
        result.TrainingTitle.Should().Be(v.Id.TrainingTitle);
        result.TrainingType.Should().Be(v.Id.TrainingType);
        result.TrainingLevel.Should().Be(v.Id.TrainingLevel);
        result.TransferInfoUkprn.Should().Be(v.Id.TransferInfoUkprn);
        result.TransferInfoProviderName.Should().Be(v.Id.TransferInfoProviderName);
        result.TransferInfoReason.Should().Be(v.Id.TransferInfoReason);
        result.TransferInfoTransferredDate.Should().Be(v.Id.TransferInfoTransferredDate);
        result.TrainingProviderName.Should().Be(v.Id.TrainingProviderName);
        result.NoOfNewApplications.Should().Be(v.NoOfNewApplications);
        result.NoOfSuccessfulApplications.Should().Be(v.NoOfSuccessfulApplications);
        result.NoOfUnsuccessfulApplications.Should().Be(v.NoOfUnsuccessfulApplications);
        result.NoOfSharedApplications.Should().Be(v.NoOfSharedApplications);
        result.NoOfAllSharedApplications.Should().Be(v.NoOfAllSharedApplications);
        result.NoOfEmployerReviewedApplications.Should().Be(v.NoOfEmployerReviewedApplications);
        result.IsTraineeship.Should().Be(v.Id.IsTraineeship);
        result.IsTaskListCompleted.Should().BeFalse();
        result.HasChosenProviderContactDetails.Should().Be(v.Id.HasChosenProviderContactDetails);
    }
}