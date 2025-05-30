using System.Linq;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.TagHelpers.FieldReviewHelper;

public class WhenShowingReviewField
{
    [Test]
    [MoqInlineAutoData(VacancyStatus.Draft, false)]
    [MoqInlineAutoData(VacancyStatus.Review, false)]
    [MoqInlineAutoData(VacancyStatus.Rejected, true)]
    [MoqInlineAutoData(VacancyStatus.Submitted, false)]
    [MoqInlineAutoData(VacancyStatus.Referred, true)]
    [MoqInlineAutoData(VacancyStatus.Live, false)]
    [MoqInlineAutoData(VacancyStatus.Closed, false)]
    [MoqInlineAutoData(VacancyStatus.Approved, false)]
    public async Task Then_Checks_Status_To_Determine_If_Field_Is_Shown(VacancyStatus vacancyStatus, bool expected,  DisplayVacancyViewModelMapper mapper, Vacancy vacancy, Recruit.Provider.Web.TagHelpers.FieldReviewHelper helper)
    {
        //Arrange
        vacancy.Status = vacancyStatus;
        foreach (var employerReviewFieldIndicator in vacancy.EmployerReviewFieldIndicators)
        {
            employerReviewFieldIndicator.IsChangeRequested = true;
        }
        var model = new VacancyPreviewViewModel();
        await mapper.MapFromVacancyAsync(model, vacancy);

        //Act
        bool actual = helper.ShowReviewField(model, model.EmployerReviewFieldIndicators.First().FieldIdentifier);

        //Assert
        actual.Should().Be(expected);
    }
}