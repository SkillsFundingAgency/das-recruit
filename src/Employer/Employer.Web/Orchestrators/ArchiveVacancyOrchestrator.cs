using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ArchiveVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators;

public class ArchiveVacancyOrchestrator(IProviderVacancyClient client,
    IRecruitVacancyClient vacancyClient,
    IUtility utility)
{
    public async Task<ArchiveViewModel> GetArchiveViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await vacancyClient.GetVacancyAsync(vrm.VacancyId);

        utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

        if (!vacancy.CanArchive)
            throw new InvalidStateException(string.Format(ErrorMessages.AdvertNotAvailableForArchive, vacancy.Title));

        var vm = new ArchiveViewModel
        {
            Title = vacancy.Title,
            Status = vacancy.Status,
            VacancyReference = vacancy.VacancyReference,
            EmployerName = vacancy.EmployerName,
            EmployerAccountId = vrm.EmployerAccountId,
            VacancyId = vrm.VacancyId
        };

        return vm;
    }

    public async Task<ArchiveViewModel> ArchiveVacancyAsync(ArchiveEditModel m, VacancyUser user)
    {
        var vacancy = await vacancyClient.GetVacancyAsync(m.VacancyId);

        utility.CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

        if (!vacancy.CanArchive)
            throw new InvalidStateException(string.Format(ErrorMessages.AdvertNotAvailableForArchive, vacancy.Title));

        var vm = new ArchiveViewModel
        {
            Title = vacancy.Title,
            Status = vacancy.Status,
            EmployerName = vacancy.EmployerName,
            EmployerAccountId = vacancy.EmployerAccountId,
            VacancyId = vacancy.Id,
            VacancyReference = vacancy.VacancyReference
        };
        await client.ArchiveVacancyAsync(vacancy.Id, user);
        return vm;
    }
}