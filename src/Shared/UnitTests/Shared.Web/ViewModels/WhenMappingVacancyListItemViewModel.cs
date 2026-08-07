using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.ViewModels;

public class WhenMappingVacancyListItemViewModel
{
    [Test, MoqAutoData]
    public void Then_The_Application_Counts_Are_Mapped(
        string employerAccountId,
        FilteringOptions filteringOptions,
        VacancyListItem item)
    {
        // act
        var result = VacancyListItemViewModel.From(item, employerAccountId, filteringOptions);

        // assert
        result.HasApplications.Should().BeTrue();
        result.NoOfApplications.Should().Be(item.Stats!.Value.Applications);
        result.Applications.Should().Be($"{item.Stats!.Value.Applications}");
        result.NoOfAllSharedApplications.Should().Be(item.Stats!.Value.AllSharedApplications);
        result.NoOfEmployerReviewedApplications.Should().Be(item.Stats!.Value.EmployerReviewedApplications);
        result.NoOfNewApplications.Should().Be(item.Stats!.Value.NewApplications);
        result.NoOfSharedApplications.Should().Be(item.Stats!.Value.SharedApplications);
        result.NoOfSuccessfulApplications.Should().Be(item.Stats!.Value.SuccessfulApplications);
        result.NoOfUnsuccessfulApplications.Should().Be(item.Stats!.Value.UnsuccessfulApplications);
    }
}