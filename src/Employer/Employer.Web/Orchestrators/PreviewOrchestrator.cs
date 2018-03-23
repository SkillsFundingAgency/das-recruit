using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class PreviewOrchestrator
    {
        private readonly IVacancyClient _client;

        public PreviewOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<PreviewVacancyViewModel> GetPreviewVacancyViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new PreviewVacancyViewModel
            {
                ApplicationInstructions = string.Empty,
                CanDelete = vacancy.CanDelete,
                CanSubmit = vacancy.CanSubmit,
                ContactName = string.Empty,
                ContactEmail = string.Empty,
                ContactTelephone = string.Empty,
                ClosingDate = vacancy.ClosingDate.Value,
                Description = string.Empty,
                EmployerName = vacancy.OrganisationName,
                EmployerWebsiteUrl = string.Empty,
                ExpectedDuration = vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value),
                HoursPerWeek = vacancy.Wage.WeeklyHours.Value,
                Location = vacancy.Location,
                NumberOfPositions = vacancy.NumberOfPositions.Value,
                PossibleStartDate = vacancy.StartDate.Value,
                ProviderName = vacancy.ProviderName,
                ProviderAddress = vacancy.ProviderAddress,
                ShortDescription = vacancy.ShortDescription,
                ThingsToConsider = string.Empty,
                Title = vacancy.Title,
                TrainingTitle = vacancy.Programme.Title,
                TrainingType = vacancy.Programme.TrainingType?.GetDisplayName(),
                TrainingLevel = vacancy.Programme.LevelName,
                Ukprn = vacancy.Ukprn,
                VacancyReferenceNumber = string.Empty,
                WageInfo = vacancy.Wage.WageAdditionalInformation,
                WageText = vacancy.Wage.FixedWageYearlyAmount?.AsMoney(),
                WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription
            };

            return vm;
        }

        public Task<bool> TrySubmitVacancyAsync(SubmitEditModel m)
        {
            return _client.SubmitVacancyAsync(m.VacancyId);
        }
    }
}
