using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ArchiveVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class ArchiveVacancyOrchestrator(IProviderVacancyClient client,
    IRecruitVacancyClient vacancyClient,
    IUtility utility)
{
    public async Task<ArchiveViewModel> GetArchiveViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await vacancyClient.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

        utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

        if (!vacancy.CanArchive)
            throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForArchive, vacancy.Title));

        var vm = new ArchiveViewModel
        {
            Title = vacancy.Title,
            Status = vacancy.Status,
            VacancyReference = vacancy.VacancyReference,
            EmployerName = vacancy.EmployerName,
            Ukprn = vrm.Ukprn,
            VacancyId = vrm.VacancyId
        };

        return vm;
    }

    public async Task<ArchiveViewModel> ArchiveVacancyAsync(ArchiveEditModel m, VacancyUser user)
    {
        var vacancy = await vacancyClient.GetVacancyAsync(m.VacancyId.GetValueOrDefault());

        utility.CheckAuthorisedAccess(vacancy, m.Ukprn);

        if (!vacancy.CanArchive)
            throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForArchive, vacancy.Title));

        var vm = new ArchiveViewModel
        {
            Title = vacancy.Title,
            Status = vacancy.Status,
            EmployerName = vacancy.EmployerName,
            VacancyReference = vacancy.VacancyReference,
            Ukprn = m.Ukprn,
            VacancyId = m.VacancyId
        };
        await client.ArchiveVacancyAsync(vacancy.Id, user);
        return vm;
    }
}