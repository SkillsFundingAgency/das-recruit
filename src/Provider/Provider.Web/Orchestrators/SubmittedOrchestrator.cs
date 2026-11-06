using Esfa.Recruit.Provider.Web.ViewModels.Submitted;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class SubmittedOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient, IUtility utility)
{
    private readonly IProviderVacancyClient _client = client;


    public async Task<VacancySubmittedConfirmationViewModel> GetVacancySubmittedConfirmationViewModelAsync(VacancyRouteModel vrm, VacancyUser vacancyUser)
    {
        var vacancy = await utility.GetAuthorisedVacancyAsync(vrm, RouteNames.Submitted_Index_Get);

        if (vacancy.Status != VacancyStatus.Submitted)
            throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotSubmittedSuccessfully, vacancy.Title));

        var isResubmit = false;
        if (vacancy.VacancyReference.HasValue)
        {
            var review = await vacancyClient.GetCurrentReferredVacancyReviewAsync(vacancy.VacancyReference.Value);
            isResubmit = review != null;
        }

        var vm = new VacancySubmittedConfirmationViewModel
        {
            Title = vacancy.Title,
            VacancyReference = vacancy.VacancyReference?.ToString(),
            IsResubmit = isResubmit,
            Ukprn = vrm.Ukprn,
            VacancyId = vrm.VacancyId
        };

        return vm;
    }
}