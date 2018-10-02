using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
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
        private readonly IRuleTemplateRunner _ruleTemplateRunner;

        public ReviewMapper(ILogger<ReviewMapper> logger,
                    IQaVacancyClient vacancyClient,
                    IGeocodeImageService mapService,
                    IMinimumWageProvider wageProvider,
                    IRuleTemplateRunner ruleTemplateRunner)
        {
            _logger = logger;
            _vacancyClient = vacancyClient;
            _mapService = mapService;
            _wageProvider = wageProvider;
            _qualifications = new Lazy<IList<string>>(() => _vacancyClient.GetCandidateQualificationsAsync().Result.QualificationTypes);
            _ruleTemplateRunner = ruleTemplateRunner;
        }

        private static readonly Dictionary<string, IEnumerable<string>> ReviewFields = new Dictionary<string, IEnumerable<string>>
        {
            //These need to be in display order
            {FieldIdResolver.ToFieldId(v => v.EmployerAccountId), new string[0]},
            {FieldIdResolver.ToFieldId(v => v.Title), new[]{VacancyReview.FieldIdentifiers.Title} },
            {FieldIdResolver.ToFieldId(v => v.EmployerName), new string[0] },
            {FieldIdResolver.ToFieldId(v => v.ShortDescription), new []{VacancyReview.FieldIdentifiers.ShortDescription} },
            {FieldIdResolver.ToFieldId(v => v.ClosingDate), new []{VacancyReview.FieldIdentifiers.ClosingDate} },
            {FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours), new []{VacancyReview.FieldIdentifiers.WorkingWeek} },
            {FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription), new []{ VacancyReview.FieldIdentifiers.WorkingWeek} },
            {FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation), new []{ VacancyReview.FieldIdentifiers.Wage}},
            {FieldIdResolver.ToFieldId(v => v.Wage.WageType),  new[]{VacancyReview.FieldIdentifiers.Wage}},
            {FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount), new []{ VacancyReview.FieldIdentifiers.Wage }},
            {FieldIdResolver.ToFieldId(v => v.Wage.Duration), new []{ VacancyReview.FieldIdentifiers.ExpectedDuration }},
            {FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit), new []{ VacancyReview.FieldIdentifiers.ExpectedDuration }},
            {FieldIdResolver.ToFieldId(v => v.StartDate), new []{ VacancyReview.FieldIdentifiers.PossibleStartDate} },
            {FieldIdResolver.ToFieldId(v => v.ProgrammeId), new []{VacancyReview.FieldIdentifiers.TrainingLevel, VacancyReview.FieldIdentifiers.Training} },
            {FieldIdResolver.ToFieldId(v => v.VacancyReference), new string[0] },
            {FieldIdResolver.ToFieldId(v => v.NumberOfPositions), new []{VacancyReview.FieldIdentifiers.NumberOfPositions} },
            {FieldIdResolver.ToFieldId(v => v.Description), new []{VacancyReview.FieldIdentifiers.VacancyDescription} },
            {FieldIdResolver.ToFieldId(v => v.TrainingDescription), new []{VacancyReview.FieldIdentifiers.TrainingDescription} },
            {FieldIdResolver.ToFieldId(v => v.OutcomeDescription), new []{VacancyReview.FieldIdentifiers.OutcomeDescription} },
            {FieldIdResolver.ToFieldId(v => v.Skills), new []{VacancyReview.FieldIdentifiers.Skills} },
            {FieldIdResolver.ToFieldId(v => v.Qualifications), new []{VacancyReview.FieldIdentifiers.Qualifications} },
            {FieldIdResolver.ToFieldId(v => v.ThingsToConsider), new []{VacancyReview.FieldIdentifiers.ThingsToConsider} },
            {FieldIdResolver.ToFieldId(v => v.EmployerDescription), new [] {VacancyReview.FieldIdentifiers.EmployerDescription }},
            {FieldIdResolver.ToFieldId(v => v.DisabilityConfident), new []{VacancyReview.FieldIdentifiers.DisabilityConfident} },
            {FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl), new []{VacancyReview.FieldIdentifiers.EmployerWebsiteUrl} },
            {FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1), new []{VacancyReview.FieldIdentifiers.EmployerAddress} },
            {FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2), new []{VacancyReview.FieldIdentifiers.EmployerAddress} },
            {FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3), new []{VacancyReview.FieldIdentifiers.EmployerAddress} },
            {FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4), new []{ VacancyReview.FieldIdentifiers.EmployerAddress} },
            {FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode), new[]{VacancyReview.FieldIdentifiers.EmployerAddress}},
            {FieldIdResolver.ToFieldId(v => v.EmployerContactEmail), new []{VacancyReview.FieldIdentifiers.EmployerContact} },
            {FieldIdResolver.ToFieldId(v => v.EmployerContactName), new []{ VacancyReview.FieldIdentifiers.EmployerContact }},
            {FieldIdResolver.ToFieldId(v => v.EmployerContactPhone), new []{VacancyReview.FieldIdentifiers.EmployerContact }},
            {FieldIdResolver.ToFieldId(v => v.TrainingProvider.Ukprn) , new []{VacancyReview.FieldIdentifiers.Provider} },
            {FieldIdResolver.ToFieldId(v => v.ApplicationInstructions), new [] {VacancyReview.FieldIdentifiers.ApplicationInstructions }},
            {FieldIdResolver.ToFieldId(v => v.ApplicationMethod), new [] {VacancyReview.FieldIdentifiers.ApplicationMethod} },
            {FieldIdResolver.ToFieldId(v => v.ApplicationUrl), new []{VacancyReview.FieldIdentifiers.ApplicationUrl} }
        };

        private static List<FieldIdentifierViewModel> GetFieldIndicators()
        {
            return new List<FieldIdentifierViewModel>
            {
                //These need to be in display order
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.Title, Text = "Title"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.ShortDescription, Text = "Brief overview of the role"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.ClosingDate, Text = "Closing date"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.WorkingWeek, Text = "Working week"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.Wage, Text = "Yearly wage"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.ExpectedDuration, Text = "Expected duration"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.PossibleStartDate, Text = "Possible start"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.TrainingLevel, Text = "Apprenticeship level"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.NumberOfPositions, Text = "Positions"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.VacancyDescription, Text = "Typical working day"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.TrainingDescription, Text = "Training description"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.OutcomeDescription, Text = "Future prospects"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.Skills, Text = "Skills"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.Qualifications, Text = "Qualifications"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.ThingsToConsider, Text = "Things to consider"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.EmployerDescription, Text = "Employer description"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.DisabilityConfident, Text = "Disability confident"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.EmployerWebsiteUrl, Text = "Employer website"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.EmployerContact, Text = "Contact details"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.EmployerAddress, Text = "Employer address"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.Provider, Text = "Training provider"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.Training, Text = "Training"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.ApplicationMethod, Text = "Application method"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.ApplicationUrl, Text = "Apply now web address"},
                new FieldIdentifierViewModel {FieldIdentifier = VacancyReview.FieldIdentifiers.ApplicationInstructions, Text = "Application process"}
            };
        }

        public async Task<ReviewViewModel> Map(VacancyReview review)
        {
            var vacancy = review.VacancySnapshot;

            var programmeTask = _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            var reviewHistoryTask = _vacancyClient.GetVacancyReviewHistoryAsync(review.VacancyReference);
            
            // Temporarily disabling counts until we remove the shard key from vacancyReviews collection
            //var approvedCountTask = _vacancyClient.GetApprovedCountAsync(vacancy.SubmittedByUser.UserId);
            //var approvedFirstTimeCountTask = _vacancyClient.GetApprovedFirstTimeCountAsync(vacancy.SubmittedByUser.UserId);
            //await Task.WhenAll(programmeTask, approvedCountTask, approvedFirstTimeCountTask);
            await Task.WhenAll(programmeTask, reviewHistoryTask);

            var programme = programmeTask.Result;

            var historiesVm = GetReviewHistoriesViewModel(reviewHistoryTask.Result);

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
                vm.NumberOfPositionsCaption = $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available";
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
                //vm.VacancyReviewsApprovedCount = approvedCountTask.Result;
                //vm.VacancyReviewsApprovedFirstTimeCount = approvedFirstTimeCountTask.Result;

                vm.FieldIdentifiers = await GetFieldIdentifiersViewModel(review);
                vm.ReviewerComment = review.ManualQaComment;
                vm.ReviewHistories = historiesVm;
                vm.IsResubmission = review.SubmissionCount > 1;

                vm.AutomatedQaResults = GetAutomatedQaResultViewModel(review);
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

        private async Task<IEnumerable<FieldIdentifierViewModel>> GetFieldIdentifiersViewModel(VacancyReview currentReview)
        {
            var vm = GetFieldIndicators();

            if (currentReview.SubmissionCount <= 1)
                return vm;

            var previousReview = await _vacancyClient.GetCurrentReferredVacancyReviewAsync(currentReview.VacancyReference);

            if (previousReview == null)
                return vm;

            //translate field name to FieldIdentifier (a single field can have multiple field identifiers)
            var updatedFieldIdentifiers = currentReview.UpdatedFieldIdentifiers?
                .SelectMany(f => ReviewFields[f])
                .Distinct()
                .ToList() ?? new List<string>();

            foreach (var field in vm)
            {
                var hasChanged = updatedFieldIdentifiers.Contains(field.FieldIdentifier);

                var previouslyChecked = previousReview.ManualQaFieldIndicators.Any(m => m.FieldIdentifier == field.FieldIdentifier &&
                                                                                        m.IsChangeRequested);

                field.FieldValueHasChanged = hasChanged;
                field.Checked = previouslyChecked;
            }

            return vm;
        }

        public List<ManualQaFieldIndicator> GetManualQaFieldIndicators(ReviewEditModel m)
        {
            return GetFieldIndicators().Select(f => new ManualQaFieldIndicator
            {
                FieldIdentifier = f.FieldIdentifier,
                IsChangeRequested = m.SelectedFieldIdentifiers.Contains(f.FieldIdentifier)
            }).OrderBy(f => f.FieldIdentifier).ToList();
        }

        

        private ReviewHistoriesViewModel GetReviewHistoriesViewModel(IEnumerable<VacancyReview> vacancyReviews)
        {
            return new ReviewHistoriesViewModel
            {
                Items = vacancyReviews.Select(h => new ReviewHistoryViewModel
                {
                    ReviewId = h.Id,
                    ReviewerName = h.ReviewedByUser.Name,
                    Outcome = h.ManualOutcome.ToString(),
                    ReviewDate = h.ReviewedDate.Value
                })
            };
        }

        private List<AutomatedQaResultViewModel> GetAutomatedQaResultViewModel(VacancyReview review)
        {
            var referredOutcomes = review.AutomatedQaOutcomeIndicators
                .Where(i => i.IsReferred)
                .Select(i => i.RuleOutcomeId)
                .ToList();

            var vm = review.AutomatedQaOutcome?.RuleOutcomes
                .SelectMany(o => o.Details)
                .Where(r => referredOutcomes.Contains(r.Id))
                .Select(r => new AutomatedQaResultViewModel
                {
                    OutcomeId = r.Id.ToString(),
                    FieldId = r.Target,
                    Checked = true,
                    Text = _ruleTemplateRunner.ToText(r.Data, FieldDisplayNameResolver.Resolve(r.Target))
                }).ToList() ?? new List<AutomatedQaResultViewModel>();

            //sort by the order of the fields on the review page
            return vm.OrderBy(v => ReviewFields.Keys.ToList().FindIndex(k => k == v.FieldId)).ToList();
        }
    }
}