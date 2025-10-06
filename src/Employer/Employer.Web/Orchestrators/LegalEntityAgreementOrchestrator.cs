using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.LegalEntityAgreement;
using Esfa.Recruit.Shared.Web.Services;

namespace Esfa.Recruit.Employer.Web.Orchestrators;

public class LegalEntityAgreementOrchestrator(ILegalEntityAgreementService legalEntityAgreementService, IUtility utility)
{
    public async Task<LegalEntityAgreementSoftStopViewModel> GetLegalEntityAgreementSoftStopViewModelAsync(
        TaskListViewModel vrm, string selectedAccountLegalEntityPublicHashedId)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);
        var accountLegalEntityPublicHashedId = !string.IsNullOrEmpty(selectedAccountLegalEntityPublicHashedId) ? selectedAccountLegalEntityPublicHashedId : vacancy.AccountLegalEntityPublicHashedId;
        var legalEntity = await legalEntityAgreementService.GetLegalEntityAsync(vrm.EmployerAccountId, accountLegalEntityPublicHashedId);
        var hasLegalEntityAgreement = await legalEntityAgreementService.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, legalEntity);
            
        return new LegalEntityAgreementSoftStopViewModel
        {                
            HasLegalEntityAgreement = hasLegalEntityAgreement,
            LegalEntityName = legalEntity.Name,
            IsTaskListComplete = utility.IsTaskListCompleted(vacancy) && !vrm.IsTaskList,
            VacancyId = vrm.VacancyId,
            EmployerAccountId = vrm.EmployerAccountId
        };
    }

    public async Task<LegalEntityAgreementHardStopViewModel> GetLegalEntityAgreementHardStopViewModelAsync(TaskListViewModel vrm)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);
        return new LegalEntityAgreementHardStopViewModel
        {
            HasLegalEntityAgreement = await legalEntityAgreementService.HasLegalEntityAgreementAsync(
                vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId),
            EmployerAccountId = vrm.EmployerAccountId,
            VacancyId = vrm.VacancyId,
        };
    }
}