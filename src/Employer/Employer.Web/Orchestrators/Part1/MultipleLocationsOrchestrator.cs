using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using AvailableWhere = Esfa.Recruit.Vacancies.Client.Domain.Entities.AvailableWhere;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1;

public interface IMultipleLocationsOrchestrator
{
    Task<AddMoreThanOneLocationViewModel> GetAddMoreThanOneLocationViewModelAsync(VacancyRouteModel vrm, bool wizard);
    Task<OrchestratorResponse> PostAddMoreThanOneLocationViewModelAsync(AddMoreThanOneLocationEditModel editModel, VacancyUser user);
}

public class MultipleLocationsOrchestrator(
    IEmployerVacancyClient employerVacancyClient,
    ILogger<MultipleLocationsOrchestrator> logger,
    IRecruitVacancyClient recruitVacancyClient,
    IUtility utility
    ) : VacancyValidatingOrchestrator<AddMoreThanOneLocationEditModel>(logger), IMultipleLocationsOrchestrator
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerAddress;
    
    protected override EntityToViewModelPropertyMappings<Vacancy, AddMoreThanOneLocationEditModel> DefineMappings()
    {
        return new EntityToViewModelPropertyMappings<Vacancy, AddMoreThanOneLocationEditModel>
        {
            { e => e.EmployerLocations, m => m.SelectedLocations }
        };
    }
    
    public async Task<AddMoreThanOneLocationViewModel> GetAddMoreThanOneLocationViewModelAsync(
            VacancyRouteModel vrm,
            bool wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.AddMoreThanOneLocation_Get);
        var employerProfile = await recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
        var allLocations = await GetAllAvailableLocationsAsync(employerProfile);
        
        var viewModel = new AddMoreThanOneLocationViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            AvailableLocations = allLocations ?? [],
            VacancyId = vrm.VacancyId,
            EmployerAccountId = vrm.EmployerAccountId,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            SelectedLocations = vacancy.EmployerLocations?.Select(l => l.ToAddressString()).ToList() ?? [],
        };
        viewModel.PageInfo.SetWizard(wizard);
        return viewModel;
    }
    
    private async Task<List<Address>> GetAllAvailableLocationsAsync(EmployerProfile employerProfile)
    {
        var employerData = await employerVacancyClient.GetEditVacancyInfoAsync(employerProfile.EmployerAccountId);
        var legalEntity = employerData.LegalEntities.First(l => l.AccountLegalEntityPublicHashedId == employerProfile.AccountLegalEntityPublicHashedId);
        var locations = new List<Address> { legalEntity.Address.ConvertToDomainAddress() };
        locations.AddRange(employerProfile.OtherLocations);
        return locations;
    }

    public async Task<OrchestratorResponse> PostAddMoreThanOneLocationViewModelAsync(AddMoreThanOneLocationEditModel editModel, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.AddMoreThanOneLocation_Post);
        var employerProfile = await recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
        var allLocations = await GetAllAvailableLocationsAsync(employerProfile);
        
        var selectedLocations = allLocations.Where(x => editModel.SelectedLocations.Contains(x.ToAddressString())).ToList();
        vacancy.EmployerLocation = null; // EmployerLocations is now the preferred field
        vacancy.EmployerLocationOption = AvailableWhere.MultipleLocations;
        vacancy.EmployerLocations = selectedLocations;
        
        return await ValidateAndExecute(
            vacancy,
            v => recruitVacancyClient.Validate(v, ValidationRules),
            async v =>
            {
                await recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
            });
    }
}