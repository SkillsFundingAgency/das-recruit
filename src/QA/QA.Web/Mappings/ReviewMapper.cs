using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Qa.Web.Mappings
{
    public class ReviewMapper
    {
        private const int MapImageWidth = 465;
        private const int MapImageHeight = 256;
        private readonly ILogger<ReviewMapper> _logger;
        private readonly IQaVacancyClient _vacancyClient;
        private readonly IGeocodeImageService _mapService;
        private readonly IMinimumWageProvider _wageProvider;
        private readonly Lazy<IList<string>> _qualifications;

        public ReviewMapper(ILogger<ReviewMapper> logger,
                    IQaVacancyClient vacancyClient, 
                    IGeocodeImageService mapService,
                    IMinimumWageProvider wageProvider)
        {
            _logger = logger;
            _vacancyClient = vacancyClient;
            _mapService = mapService;
            _wageProvider = wageProvider;
            _qualifications = new Lazy<IList<string>>(() => _vacancyClient.GetCandidateQualificationsAsync().Result.QualificationTypes);
        }
        
        public async Task<ReviewViewModel> MapFromVacancy(Vacancy vacancy)
        {
            var programmeTask = _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);
            var approvedCountTask = _vacancyClient.GetApprovedCountAsync(vacancy.SubmittedByUser.UserId);
            var approvedFirstTimeCountTask = _vacancyClient.GetApprovedFirstTimeCountAsync(vacancy.SubmittedByUser.UserId);

            await Task.WhenAll(programmeTask, approvedCountTask, approvedFirstTimeCountTask);

            var programme = programmeTask.Result;

            var vm = new ReviewViewModel();

            try
            {
                vm.SubmittedByName = vacancy.SubmittedByUser.Name;
                vm.SubmittedByEmail = vacancy.SubmittedByUser.Email;
                vm.ApplicationInstructions = vacancy.ApplicationInstructions;
                vm.ApplicationMethod = vacancy.ApplicationMethod.Value;
                vm.ApplicationUrl = vacancy.ApplicationUrl;
                vm.ContactName = vacancy.EmployerContactName;
                vm.ContactEmail = vacancy.EmployerContactEmail;
                vm.ContactTelephone = vacancy.EmployerContactPhone;
                vm.ClosingDate = vacancy.ClosingDate?.AsGdsDate();
                vm.EmployerDescription = vacancy.EmployerDescription;
                vm.EmployerName = vacancy.EmployerName;
                vm.EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl;
                SetEmployerAddressElements(vm, vacancy);
                vm.IsDisabilityConfident = vacancy.IsDisabilityConfident;
                vm.NumberOfPositionsCaption =  $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available";
                vm.OutcomeDescription = vacancy.OutcomeDescription;
                vm.PossibleStartDate = vacancy.StartDate?.AsGdsDate();
                vm.ProviderName = vacancy.TrainingProvider.Name;
                vm.Qualifications = vacancy.Qualifications.SortQualifications(_qualifications.Value).AsText();
                vm.ShortDescription = vacancy.ShortDescription;
                vm.Skills = vacancy.Skills ?? Enumerable.Empty<string>();
                vm.ThingsToConsider = vacancy.ThingsToConsider;
                vm.Title = vacancy.Title;
                vm.TrainingDescription = vacancy.TrainingDescription;
                vm.VacancyDescription = vacancy.Description;
                vm.VacancyReferenceNumber = $"VAC{vacancy.VacancyReference.ToString()}";
                vm.TrainingTitle = programme.Title;
                vm.TrainingType = programme.ApprenticeshipType.GetDisplayName();
                vm.TrainingLevel = programme.Level.GetDisplayName();
                vm.ExpectedDuration = (vacancy.Wage.DurationUnit.HasValue && vacancy.Wage.Duration.HasValue)
                    ? vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value)
                    : null;
                vm.HoursPerWeek = $"{vacancy.Wage.WeeklyHours:0.##}";
                vm.WageInfo = vacancy.Wage.WageAdditionalInformation;
                vm.WageText = vacancy.StartDate.HasValue
                    ? vacancy.Wage.ToText(
                        () => _wageProvider.GetNationalMinimumWageRange(vacancy.StartDate.Value),
                        () => _wageProvider.GetApprenticeNationalMinimumWage(vacancy.StartDate.Value))
                    : null;
                vm.WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription;
                vm.SubmittedDate = vacancy.SubmittedDate.Value;
                vm.VacancyReviewsApprovedCount = approvedCountTask.Result;
                vm.VacancyReviewsApprovedFirstTimeCount = approvedFirstTimeCountTask.Result;
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, "Unable to map vacancy to view model. Unexpected null fields.");
                throw;
            }
            
            return vm;
        }
        
        private void SetEmployerAddressElements(ReviewViewModel vm, Vacancy vacancy)
        {
            vm.MapUrl = vacancy.EmployerLocation.HasGeocode
                ? _mapService.GetMapImageUrl(vacancy.EmployerLocation.Latitude.ToString(),
                    vacancy.EmployerLocation.Longitude.ToString(), MapImageWidth, MapImageHeight)
                : _mapService.GetMapImageUrl(vacancy.EmployerLocation.Postcode, MapImageWidth, MapImageHeight);
            vm.EmployerAddressElements = new[]
                {
                    vacancy.EmployerLocation.AddressLine1,
                    vacancy.EmployerLocation.AddressLine2,
                    vacancy.EmployerLocation.AddressLine3,
                    vacancy.EmployerLocation.AddressLine4,
                    vacancy.EmployerLocation.Postcode
                }
                .Where(x => !string.IsNullOrEmpty(x));
        }
    }
}