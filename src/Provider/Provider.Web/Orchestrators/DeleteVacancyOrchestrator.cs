using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.DeleteVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class DeleteVacancyOrchestrator(
    IProviderVacancyClient client,
    IRecruitVacancyClient vacancyClient,
    IUtility utility)
{
    public async Task<DeleteViewModel> GetDeleteViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await vacancyClient.GetVacancyAsync(vrm.VacancyId.Value);

        utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

        if (!vacancy.CanDelete)
            throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

        var vm = new DeleteViewModel
        {
            Title = vacancy.Title,
            Status = vacancy.Status
        };

        return vm;
    }

    public async Task<DeleteViewModel> DeleteVacancyAsync(DeleteEditModel m, VacancyUser user)
    {
        var vacancy = await vacancyClient.GetVacancyAsync(m.VacancyId.Value);

        utility.CheckAuthorisedAccess(vacancy, m.Ukprn);

        if (!vacancy.CanDelete)
            throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

        var vm = new DeleteViewModel
        {
            Title = vacancy.Title,
            Status = vacancy.Status,
            VacancyReference = vacancy.VacancyReference
        };
        await client.DeleteVacancyAsync(vacancy.Id, user);
        return vm;
    }
}