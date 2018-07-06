using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Employer.Web.Views;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class WageOrchestrator : EntityValidatingOrchestrator<Vacancy, WageEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Duration | VacancyRuleSet.WorkingWeekDescription | VacancyRuleSet.WeeklyHours | VacancyRuleSet.Wage | VacancyRuleSet.MinimumWage;
        private readonly IEmployerVacancyClient _client;

        public WageOrchestrator(IEmployerVacancyClient client, ILogger<WageOrchestrator> logger) : base(logger)
        {
            _client = client;
        }

        public async Task<WageViewModel> GetWageViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.Wage_Get);
            
            var vm = new WageViewModel
            {
                Duration = vacancy.Wage?.Duration?.ToString(),
                DurationUnit = vacancy.Wage?.DurationUnit ?? DurationUnit.Year,
                WorkingWeekDescription = vacancy.Wage?.WorkingWeekDescription,
                WeeklyHours = $"{vacancy.Wage?.WeeklyHours:0.##}",
                WageType = vacancy.Wage?.WageType ?? WageType.FixedWage,
                FixedWageYearlyAmount = vacancy.Wage?.FixedWageYearlyAmount?.AsMoney(),
                WageAdditionalInformation = vacancy.Wage?.WageAdditionalInformation,
                CancelButtonRouteParameters = Utility.GetCancelButtonRouteParametersForVacancy(vacancy, PreviewAnchors.ApprenticeshipSummarySection),
                InWizardMode = vacancy.HasCompletedPart1 == false
            };
            
            return vm;
        }

        public async Task<WageViewModel> GetWageViewModelAsync(WageEditModel m)
        {
            var vm = await GetWageViewModelAsync((VacancyRouteModel)m);

            vm.Duration = m.Duration;
            vm.DurationUnit = m.DurationUnit;
            vm.WorkingWeekDescription = m.WorkingWeekDescription;
            vm.WeeklyHours = m.WeeklyHours;
            vm.WageType = m.WageType;
            vm.FixedWageYearlyAmount = m.FixedWageYearlyAmount;
            vm.WageAdditionalInformation = m.WageAdditionalInformation;
            
            return vm;
        }

        public async Task<OrchestratorResponse<VacancyRouteParameters>> PostWageEditModelAsync(WageEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.Wage_Post);

            vacancy.Wage = new Wage
            {
                Duration = int.TryParse(m.Duration, out int duration) == true ? duration : default(int?),
                DurationUnit = m.DurationUnit,
                WorkingWeekDescription = m.WorkingWeekDescription,
                WeeklyHours = m.WeeklyHours.AsDecimal(2),
                WageType = m.WageType,
                FixedWageYearlyAmount = (m.WageType == WageType.FixedWage) ? m.FixedWageYearlyAmount?.AsMoney() : null,
                WageAdditionalInformation = m.WageAdditionalInformation
            };

            return await ValidateAndExecute(
                vacancy, 
                v => _client.Validate(v, ValidationRules),
                async v =>
                {
                    await _client.UpdateVacancyAsync(vacancy, user);
                    return Utility.GetRedirectRouteParametersForVacancy(vacancy, PreviewAnchors.ApprenticeshipSummarySection, RouteNames.Wage_Post);
                });
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, WageEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, WageEditModel>
            {
                { e => e.Wage.Duration, vm => vm.Duration},
                { e => e.Wage.DurationUnit, vm => vm.DurationUnit},
                { e => e.Wage.WorkingWeekDescription, vm => vm.WorkingWeekDescription},
                { e => e.Wage.WeeklyHours, vm => vm.WeeklyHours},
                { e => e.Wage.WageType, vm => vm.WageType},
                { e => e.Wage.FixedWageYearlyAmount, vm => vm.FixedWageYearlyAmount},
                { e => e.Wage.WageAdditionalInformation, vm => vm.WageAdditionalInformation}
            };

            return mappings;
        }
    }
}
