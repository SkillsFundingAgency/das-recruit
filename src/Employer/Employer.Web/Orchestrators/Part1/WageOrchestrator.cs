using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class WageOrchestrator : EntityValidatingOrchestrator<Vacancy, WageEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Duration | VacancyRuleSet.WorkingWeekDescription | VacancyRuleSet.WeeklyHours | VacancyRuleSet.Wage | VacancyRuleSet.MinimumWage;
        private readonly IVacancyClient _client;
        private readonly ILogger<WageOrchestrator> _logger;

        public WageOrchestrator(IVacancyClient client, ILogger<WageOrchestrator> logger) : base(logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<WageViewModel> GetWageViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var vm = new WageViewModel
            {
                Duration = vacancy.Wage?.Duration?.ToString(),
                DurationUnit = vacancy.Wage?.DurationUnit ?? DurationUnit.Year,
                WorkingWeekDescription = vacancy.Wage?.WorkingWeekDescription,
                WeeklyHours = $"{vacancy.Wage?.WeeklyHours:0.##}",
                WageType = vacancy.Wage?.WageType ?? WageType.FixedWage,
                FixedWageYearlyAmount = vacancy.Wage?.FixedWageYearlyAmount?.AsMoney(),
                WageAdditionalInformation = vacancy.Wage?.WageAdditionalInformation
            };
            
            return vm;
        }

        public async Task<WageViewModel> GetWageViewModelAsync(WageEditModel m)
        {
            var vm = await GetWageViewModelAsync(m.VacancyId);

            vm.Duration = m.Duration;
            vm.DurationUnit = m.DurationUnit;
            vm.WorkingWeekDescription = m.WorkingWeekDescription;
            vm.WeeklyHours = m.WeeklyHours;
            vm.WageType = m.WageType;
            vm.FixedWageYearlyAmount = m.FixedWageYearlyAmount;
            vm.WageAdditionalInformation = m.WageAdditionalInformation;
            
            return vm;
        }

        public async Task<OrchestratorResponse> PostWageEditModelAsync(WageEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            vacancy.Wage = new Wage
            {
                Duration = int.TryParse(m.Duration, out int duration) == true ? duration : default(int?),
                DurationUnit = m.DurationUnit,
                WorkingWeekDescription = m.WorkingWeekDescription,
                WeeklyHours = m.WeeklyHours.AsDecimal(),
                WageType = m.WageType,
                FixedWageYearlyAmount = m.FixedWageYearlyAmount?.AsMoney(),
                WageAdditionalInformation = m.WageAdditionalInformation
            };

            return await ValidateAndExecute(
                vacancy, 
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateVacancyAsync(vacancy)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, WageEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, WageEditModel>();

            mappings.Add(e => e.Wage.Duration, vm => vm.Duration);
            mappings.Add(e => e.Wage.DurationUnit, vm => vm.DurationUnit);
            mappings.Add(e => e.Wage.WorkingWeekDescription, vm => vm.WorkingWeekDescription);
            mappings.Add(e => e.Wage.WeeklyHours, vm => vm.WeeklyHours);
            mappings.Add(e => e.Wage.WageType, vm => vm.WageType);
            mappings.Add(e => e.Wage.FixedWageYearlyAmount, vm => vm.FixedWageYearlyAmount);
            mappings.Add(e => e.Wage.WageAdditionalInformation, vm => vm.WageAdditionalInformation);

            return mappings;
        }
    }
}
