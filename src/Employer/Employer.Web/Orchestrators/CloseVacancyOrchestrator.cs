using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Orchestrators;

public class CloseVacancyOrchestrator(IRecruitVacancyClient vacancyClient, IUtility utility)
{
    public async Task<CloseViewModel> GetCloseViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await vacancyClient.GetVacancyAsync(vrm.VacancyId);

        utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

        if (!vacancy.CanClose)
            throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForClosing, vacancy.Title));

        var vm = new CloseViewModel
        {
            VacancyId = vrm.VacancyId,
            EmployerAccountId = vrm.EmployerAccountId,
            Title = vacancy.Title,
            VacancyReference = vacancy.VacancyReference.Value.ToString()
        };

        return vm;
    }

    public async Task<OrchestratorResponse<VacancyInfo>> CloseVacancyAsync(CloseEditModel m, VacancyUser user)
    {
        var vacancy = await vacancyClient.GetVacancyAsync(m.VacancyId);

        utility.CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

        if (!vacancy.CanClose)
            throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForClosing, vacancy.Title));

        await vacancyClient.CloseVacancyAsync(vacancy.Id, user, ClosureReason.Manual);

        return new OrchestratorResponse<VacancyInfo>(new VacancyInfo
        {
            Id = vacancy.Id,
            VacancyReference = vacancy.VacancyReference.Value.ToString(),
            Title = vacancy.Title
        });
    }
}