using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.TagHelpers
{
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
        public async Task Then_Checks_Status_To_Determine_If_Field_Is_Shown(VacancyStatus vacancyStatus, bool expected,  DisplayVacancyViewModelMapper mapper, Vacancy vacancy, Esfa.Recruit.Employer.Web.TagHelpers.FieldReviewHelper helper)
        {
            //Arrange
            vacancy.Status = vacancyStatus;
            foreach (var employerReviewFieldIndicator in vacancy.EmployerReviewFieldIndicators)
            {
                employerReviewFieldIndicator.IsChangeRequested = true;
            }
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            //Act
            var actual = helper.ShowReviewField(model, model.ProviderReviewFieldIndicators.First().FieldIdentifier);

            //Assert
            actual.Should().Be(expected);
        }
    }
}