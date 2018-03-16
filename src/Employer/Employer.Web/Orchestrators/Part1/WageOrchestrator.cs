using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class WageOrchestrator
    {
        private readonly IVacancyClient _client;

        public WageOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<WageViewModel> GetWageViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

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

        public async Task PostWageEditModelAsync(WageEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            vacancy.Wage = new Wage
            {
                Duration = int.Parse(m.Duration),
                DurationUnit = m.DurationUnit,
                WorkingWeekDescription = m.WorkingWeekDescription,
                WeeklyHours = m.WeeklyHours.AsDecimal(),
                WageType = m.WageType,
                FixedWageYearlyAmount = m.FixedWageYearlyAmount?.AsMoney(),
                WageAdditionalInformation = m.WageAdditionalInformation
            };
            
            await _client.UpdateVacancyAsync(vacancy, false);
        }
    }
}
