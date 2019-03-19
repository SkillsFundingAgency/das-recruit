using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class EmployerNameOrchestrator : EntityValidatingOrchestrator<Vacancy, EmployerNameEditModel>
    {
        private readonly IEmployerVacancyClient _employerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly ILegalEntityAgreementService _legalEntityAgreementService;
        private readonly ILogger<EmployerNameOrchestrator> _logger;
        
        public EmployerNameOrchestrator(IEmployerVacancyClient employerVacancyClient, IRecruitVacancyClient recruitVacancyClient, 
            ILegalEntityAgreementService legalEntityAgreementService, ILogger<EmployerNameOrchestrator> logger)
            : base(logger) 
        {
            _employerVacancyClient = employerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _legalEntityAgreementService = legalEntityAgreementService;
            _logger = logger;
        }

        public async Task<EmployerNameViewModel> GetEmployerNameViewModelAsync(VacancyRouteModel vrm, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_employerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Employer_Get);
                
            var getEmployerDataTask = _employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);
            var getEmployerProfileTask = _employerVacancyClient.GetEmployerProfileAsync(vrm.EmployerAccountId, vacancy.LegalEntityId);
         
            await Task.WhenAll(getEmployerDataTask, getEmployerProfileTask);

            var editVacancyInfo = getEmployerDataTask.Result;
            var employerProfile = getEmployerProfileTask.Result;

            var legalEntity = editVacancyInfo.LegalEntities.FirstOrDefault(l => l.LegalEntityId == vacancy.LegalEntityId);

            var vm = new EmployerNameViewModel 
            {
                HasOnlyOneOrganisation = editVacancyInfo.LegalEntities.Count() == 1,
                LegalEntityName = legalEntity.Name,
                ExistingTradingName = employerProfile.TradingName,
                PageInfo = Utility.GetPartOnePageInfo(vacancy)
            };

            if (vacancy.EmployerNameOption.HasValue) 
                vm.SelectedEmployerNameOption = Enum.Parse<EmployerNameOptionViewModel>(vacancy.EmployerNameOption.ToString());
                
            return vm;
        }

        public async Task<OrchestratorResponse> PostEmployerNameEditModelAsync(EmployerNameEditModel model, VacancyUser user)
        {
            var ValidationRules = VacancyRuleSet.EmployerNameOption;

            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _employerVacancyClient, _recruitVacancyClient, model, RouteNames.EmployerName_Post);

            if (model.SelectedEmployerNameOption.HasValue)
            {
                vacancy.EmployerNameOption =  
                    model.SelectedEmployerNameOption == EmployerNameOptionViewModel.RegisteredName 
                    ? EmployerNameOption.RegisteredName : EmployerNameOption.TradingName;
            }

            // temporarily set the employer name for validation
            if (model.SelectedEmployerNameOption == EmployerNameOptionViewModel.NewTradingName)
            {
                ValidationRules = VacancyRuleSet.EmployerNameOption | VacancyRuleSet.TradingName;
                vacancy.EmployerName = model.NewTradingName;
            }

            return await ValidateAndExecute(
                vacancy, 
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                async v => 
                { 
                    if (model.SelectedEmployerNameOption == EmployerNameOptionViewModel.NewTradingName)
                    {
                        await UpdateEmployerProfileAsync(vacancy, model.NewTradingName, user);
                    }
                    //reset employer name, this will be updated on submit
                    vacancy.EmployerName = null;
                    await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                }
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EmployerNameEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerNameEditModel>();

            mappings.Add(v => v.EmployerName, vm => vm.NewTradingName);
            mappings.Add(v => v.EmployerNameOption, vm => vm.SelectedEmployerNameOption);

            return mappings;
        }

        private async Task UpdateEmployerProfileAsync(Vacancy vacancy, string tradingName, VacancyUser user)
        {
            var employerProfile =
                await _employerVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.LegalEntityId);

            if (employerProfile == null)
            {
                throw new NullReferenceException($"No Employer Profile was found for employerAccount: {vacancy.EmployerAccountId}, legalEntity: {vacancy.LegalEntityId}");
            }

            if (employerProfile.TradingName != tradingName)
            {
                employerProfile.TradingName = tradingName;
                await _employerVacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
            }
        }
    }
}