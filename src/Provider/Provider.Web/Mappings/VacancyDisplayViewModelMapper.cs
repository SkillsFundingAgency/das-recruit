﻿using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Mappings
{
    public class DisplayVacancyViewModelMapper
    {
        private const int MapImageWidth = 465;
        private const int MapImageHeight = 256;
        private readonly IGeocodeImageService _mapService;
        private readonly ExternalLinksConfiguration _externalLinksConfiguration;
        private readonly IRecruitVacancyClient _vacancyClient;

        public DisplayVacancyViewModelMapper(
                IGeocodeImageService mapService,
                IOptions<ExternalLinksConfiguration> externalLinksOptions,
                IRecruitVacancyClient vacancyClient)
        {
            _mapService = mapService;
            _externalLinksConfiguration = externalLinksOptions.Value;
            _vacancyClient = vacancyClient;
        }

        public async Task MapFromVacancyAsync(DisplayVacancyViewModel vm, Vacancy vacancy)
        {
            var programme = await _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            var allQualifications = await _vacancyClient.GetCandidateQualificationsAsync();

            vm.ApplicationMethod = vacancy.ApplicationMethod;
            vm.ApplicationInstructions = vacancy.ApplicationInstructions;
            vm.ApplicationUrl = vacancy.ApplicationUrl;
            vm.CanDelete = vacancy.CanDelete;
            vm.CanSubmit = vacancy.CanSubmit;
            vm.ClosingDate = vacancy.ClosingDate?.AsGdsDate();
            vm.EmployerDescription = vacancy.EmployerDescription;
            vm.EmployerName = await _vacancyClient.GetEmployerNameAsync(vacancy);
            vm.EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl;
            vm.EmployerAddressElements = Enumerable.Empty<string>();
            vm.FindAnApprenticeshipUrl = _externalLinksConfiguration.FindAnApprenticeshipUrl;
            vm.IsAnonymous = vacancy.IsAnonymous;
            vm.NumberOfPositions = vacancy.NumberOfPositions?.ToString();
            vm.NumberOfPositionsCaption = vacancy.NumberOfPositions.HasValue
                ? $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available"
                : null;
            vm.OutcomeDescription = vacancy.OutcomeDescription;
            vm.PossibleStartDate = vacancy.StartDate?.AsGdsDate();
            vm.ProviderContactName = vacancy.ProviderContact?.Name;
            vm.ProviderContactEmail = vacancy.ProviderContact?.Email;
            vm.ProviderContactTelephone = vacancy.ProviderContact?.Phone;
            vm.ProviderName = vacancy.TrainingProvider?.Name;
            vm.Qualifications = vacancy.Qualifications.SortQualifications(allQualifications).AsText();
            vm.ShortDescription = vacancy.ShortDescription;
            vm.Skills = vacancy.Skills ?? Enumerable.Empty<string>();
            vm.ThingsToConsider = vacancy.ThingsToConsider;
            vm.Title = vacancy.Title;
            vm.TrainingDescription = vacancy.TrainingDescription;
            vm.VacancyDescription = vacancy.Description;
            vm.VacancyReferenceNumber = vacancy.VacancyReference.HasValue
                                        ? $"VAC{vacancy.VacancyReference.ToString()}"
                                        : string.Empty;
            vm.IsDisabilityConfident = vacancy.IsDisabilityConfident;

            if (vacancy.EmployerLocation != null)
            {
                vm.MapUrl = MapImageHelper.GetEmployerLocationMapUrl(vacancy, _mapService, MapImageWidth, MapImageHeight);

                vm.EmployerAddressElements = vacancy.EmployerAddressForDisplay();
            }

            if (vacancy.ProgrammeId != null)
            {
                vm.TrainingTitle = programme?.Title;
                vm.TrainingType = programme?.ApprenticeshipType.GetDisplayName();
                vm.TrainingLevel = programme?.Level.GetDisplayName();
            }

            if (vacancy.Wage != null)
            {
                vm.ExpectedDuration = (vacancy.Wage.DurationUnit.HasValue && vacancy.Wage.Duration.HasValue)
                    ? vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value)
                    : null;
                vm.HoursPerWeek = $"{vacancy.Wage.WeeklyHours:0.##}";
                vm.WageInfo = vacancy.Wage.WageAdditionalInformation;
                vm.WageText = vacancy.StartDate.HasValue ? vacancy.Wage.ToText(vacancy.StartDate) : null;
                vm.WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription;
            }
        }
    }
}