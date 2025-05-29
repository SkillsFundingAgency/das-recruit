using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.UnitTests.Mappings.DisplayVacancyViewModelMapperTests;

public class WhenMappingQualifications
{
    [Test, MoqAutoData]
    public async Task Then_They_Are_Ordered_Correctly(
        Vacancy vacancy,
        DisplayVacancyViewModel viewModel,
        [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
        DisplayVacancyViewModelMapper sut)
    {
        // arrange
        vacancy.Qualifications = [
            new Qualification { Grade = "A", Subject = "Biology", QualificationType = "A Level", Weighting = QualificationWeighting.Essential },
            new Qualification { Grade = "B", Subject = "Geography", QualificationType = "GCSE", Weighting = QualificationWeighting.Desired },
            new Qualification { Grade = "A", Subject = "Physics", QualificationType = "GCSE", Weighting = QualificationWeighting.Essential },
        ];
        
        List<string> allQualifications = ["GCSE", "A Level", "Diploma"];
        vacancyClient.Setup(x => x.GetCandidateQualificationsAsync()).ReturnsAsync(allQualifications);
        
        // act
        await sut.MapFromVacancyAsync(viewModel, vacancy);

        // assert
        viewModel.Qualifications.Should().HaveCount(3);
        viewModel.Qualifications![0].Should().Contain("Physics");
        viewModel.Qualifications![1].Should().Contain("Geography");
        viewModel.Qualifications![2].Should().Contain("Biology");
    }
    
    [Test, MoqAutoData]
    public async Task Then_They_Are_Split_By_Type_Correctly(
        Vacancy vacancy,
        DisplayVacancyViewModel viewModel,
        [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
        DisplayVacancyViewModelMapper sut)
    {
        // arrange
        vacancy.Qualifications = [
            new Qualification { Grade = "A", Subject = "Biology", QualificationType = "A Level", Weighting = QualificationWeighting.Essential },
            new Qualification { Grade = "B", Subject = "Geography", QualificationType = "GCSE", Weighting = QualificationWeighting.Desired },
            new Qualification { Grade = "A", Subject = "Physics", QualificationType = "GCSE", Weighting = QualificationWeighting.Essential },
        ];
        
        List<string> allQualifications = ["GCSE", "A Level", "Diploma"];
        vacancyClient.Setup(x => x.GetCandidateQualificationsAsync()).ReturnsAsync(allQualifications);
        
        // act
        await sut.MapFromVacancyAsync(viewModel, vacancy);

        // assert
        viewModel.QualificationsEssential.Should().HaveCount(2);
        viewModel.QualificationsDesired.Should().HaveCount(1);

        viewModel.QualificationsDesired![0].Should().Contain("Geography");
        viewModel.QualificationsEssential![0].Should().Contain("Physics");
        viewModel.QualificationsEssential![1].Should().Contain("Biology");
    }
}