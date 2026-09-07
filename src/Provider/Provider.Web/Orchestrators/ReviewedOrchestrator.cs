using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Reviewed;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class ReviewedOrchestrator(IProviderVacancyClient client, IUtility utility)
{
    public async Task<VacancyReviewedConfirmationViewModel> GetVacancyReviewedOrchestratorConfirmationViewModelAsync(VacancyRouteModel vrm, VacancyUser vacancyUser)
    {
        var vacancy = await utility.GetAuthorisedVacancyAsync(vrm, RouteNames.Reviewed_Index_Get);
        var employer = await client.GetProviderEmployerVacancyDataAsync(vrm.Ukprn, vacancy.EmployerAccountId);

        if (vacancy.Status != VacancyStatus.Review)
            throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotReviewedSuccessfully, vacancy.Title));

        var vm = new VacancyReviewedConfirmationViewModel
        {
            Title = vacancy.Title,
            VacancyReference = vacancy.VacancyReference?.ToString(),
            EmployerName = employer.Name,
            IsResubmit = vacancy.ReviewCount > 1,
            Ukprn = vrm.Ukprn,
            VacancyId = vrm.VacancyId
        };

        return vm;
    }
}