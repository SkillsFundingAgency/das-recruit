﻿using System;
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
        private readonly IRuleMessageTemplateRunner _ruleTemplateRunner;
        private readonly IReviewSummaryService _reviewSummaryService;

        public ReviewMapper(ILogger<ReviewMapper> logger,
                    IQaVacancyClient vacancyClient,
                    IGeocodeImageService mapService,
                    IMinimumWageProvider wageProvider,
                    IRuleMessageTemplateRunner ruleTemplateRunner,
                    IReviewSummaryService reviewSummaryService)
        {
            _logger = logger;
            _vacancyClient = vacancyClient;
            _mapService = mapService;
            _wageProvider = wageProvider;
            _qualifications = new Lazy<IList<string>>(() => _vacancyClient.GetCandidateQualificationsAsync().Result.QualificationTypes);
            _ruleTemplateRunner = ruleTemplateRunner;
            _reviewSummaryService = reviewSummaryService;
        }

        private static readonly Dictionary<string, IEnumerable<string>> ReviewFields = new Dictionary<string, IEnumerable<string>>
        {
            //These need to be in display order
            { FieldIdResolver.ToFieldId(v => v.EmployerAccountId), new string[0]},
            { FieldIdResolver.ToFieldId(v => v.Title), new[]{FieldIdentifiers.Title} },
            { FieldIdResolver.ToFieldId(v => v.EmployerName), new string[0] },
            { FieldIdResolver.ToFieldId(v => v.ShortDescription), new []{FieldIdentifiers.ShortDescription} },
            { FieldIdResolver.ToFieldId(v => v.ClosingDate), new []{FieldIdentifiers.ClosingDate} },
            { FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours), new []{FieldIdentifiers.WorkingWeek} },
            { FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription), new []{ FieldIdentifiers.WorkingWeek} },
            { FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation), new []{ FieldIdentifiers.Wage}},
            { FieldIdResolver.ToFieldId(v => v.Wage.WageType),  new[]{FieldIdentifiers.Wage}},
            { FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount), new []{ FieldIdentifiers.Wage }},
            { FieldIdResolver.ToFieldId(v => v.Wage.Duration), new []{ FieldIdentifiers.ExpectedDuration }},
            { FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit), new []{ FieldIdentifiers.ExpectedDuration }},
            { FieldIdResolver.ToFieldId(v => v.StartDate), new []{ FieldIdentifiers.PossibleStartDate} },
            { FieldIdResolver.ToFieldId(v => v.ProgrammeId), new []{FieldIdentifiers.TrainingLevel, FieldIdentifiers.Training} },
            { FieldIdResolver.ToFieldId(v => v.VacancyReference), new string[0] },
            { FieldIdResolver.ToFieldId(v => v.NumberOfPositions), new []{FieldIdentifiers.NumberOfPositions} },
            { FieldIdResolver.ToFieldId(v => v.Description), new []{FieldIdentifiers.VacancyDescription} },
            { FieldIdResolver.ToFieldId(v => v.TrainingDescription), new []{FieldIdentifiers.TrainingDescription} },
            { FieldIdResolver.ToFieldId(v => v.OutcomeDescription), new []{FieldIdentifiers.OutcomeDescription} },
            { FieldIdResolver.ToFieldId(v => v.Skills), new []{FieldIdentifiers.Skills} },
            { FieldIdResolver.ToFieldId(v => v.Qualifications), new []{FieldIdentifiers.Qualifications} },
            { FieldIdResolver.ToFieldId(v => v.ThingsToConsider), new []{FieldIdentifiers.ThingsToConsider} },
            { FieldIdResolver.ToFieldId(v => v.EmployerDescription), new [] {FieldIdentifiers.EmployerDescription }},
            { FieldIdResolver.ToFieldId(v => v.DisabilityConfident), new []{FieldIdentifiers.DisabilityConfident} },
            { FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl), new []{FieldIdentifiers.EmployerWebsiteUrl} },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1), new []{FieldIdentifiers.EmployerAddress} },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2), new []{FieldIdentifiers.EmployerAddress} },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3), new []{FieldIdentifiers.EmployerAddress} },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4), new []{ FieldIdentifiers.EmployerAddress} },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode), new[]{FieldIdentifiers.EmployerAddress}},
            { FieldIdResolver.ToFieldId(v => v.EmployerContact.Name), new []{ FieldIdentifiers.EmployerContact }},
            { FieldIdResolver.ToFieldId(v => v.EmployerContact.Email), new []{FieldIdentifiers.EmployerContact} },
            { FieldIdResolver.ToFieldId(v => v.EmployerContact.Phone), new []{FieldIdentifiers.EmployerContact }},
            { FieldIdResolver.ToFieldId(v => v.TrainingProvider.Ukprn) , new []{FieldIdentifiers.Provider} },
            { FieldIdResolver.ToFieldId(v => v.ApplicationInstructions), new [] {FieldIdentifiers.ApplicationInstructions }},
            { FieldIdResolver.ToFieldId(v => v.ApplicationMethod), new [] {FieldIdentifiers.ApplicationMethod} },
            { FieldIdResolver.ToFieldId(v => v.ApplicationUrl), new []{FieldIdentifiers.ApplicationUrl} }
        };

        private static List<FieldIdentifierViewModel> GetFieldIndicators()
        {
            return new List<FieldIdentifierViewModel>
            {
                //These need to be in display order
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Title, Text = "Title" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ShortDescription, Text = "Brief overview" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ClosingDate, Text = "Closing date" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.WorkingWeek, Text = "Working week" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Wage, Text = "Yearly wage" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ExpectedDuration, Text = "Expected duration" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.PossibleStartDate, Text = "Possible start" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.TrainingLevel, Text = "Apprenticeship level" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.NumberOfPositions, Text = "Positions" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.VacancyDescription, Text = "What will you do in your working day" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.TrainingDescription, Text = "The training you will be getting" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.OutcomeDescription, Text = "What to expect at the end of your apprenticeship" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Skills, Text = "Skills" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Qualifications, Text = "Qualifications" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ThingsToConsider, Text = "Things to consider" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.EmployerDescription, Text = "Employer description" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.DisabilityConfident, Text = "Disability confident" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.EmployerWebsiteUrl, Text = "Employer website" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.EmployerContact, Text = "Contact details" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.EmployerAddress, Text = "Employer address" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Provider, Text = "Training provider" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Training, Text = "Training" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ApplicationMethod, Text = "Application method" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ApplicationUrl, Text = "Apply now web address" },
                new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ApplicationInstructions, Text = "Application process" }
            };
        }

        public async Task<ReviewViewModel> Map(VacancyReview review)
        {
            var vacancy = review.VacancySnapshot;

            var programmeTask = _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            var reviewHistoryTask = _vacancyClient.GetVacancyReviewHistoryAsync(review.VacancyReference);
            
            var approvedCountTask = _vacancyClient.GetApprovedCountAsync(vacancy.SubmittedByUser.UserId);

            var approvedFirstTimeCountTask = _vacancyClient.GetApprovedFirstTimeCountAsync(vacancy.SubmittedByUser.UserId);

            var reviewSummaryTask = _reviewSummaryService.GetReviewSummaryViewModelAsync(review.Id, 
                    ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators());

            await Task.WhenAll(programmeTask, approvedCountTask, approvedFirstTimeCountTask, reviewHistoryTask, reviewSummaryTask);

            var programme = programmeTask.Result;

            var historiesVm = GetReviewHistoriesViewModel(reviewHistoryTask.Result);

            var wagePeriod = _wageProvider.GetWagePeriod(vacancy.StartDate.Value);

            var vm = new ReviewViewModel();
            vm.Review = reviewSummaryTask.Result;
            try
            {
                vm.SubmittedByName = vacancy.SubmittedByUser.Name;
                vm.SubmittedByEmail = vacancy.SubmittedByUser.Email;
                vm.ApplicationInstructions = vacancy.ApplicationInstructions;
                vm.ApplicationMethod = vacancy.ApplicationMethod.Value;
                vm.ApplicationUrl = vacancy.ApplicationUrl;
                vm.ContactName = vacancy.EmployerContact?.Name;
                vm.ContactEmail = vacancy.EmployerContact?.Email;
                vm.ContactTelephone = vacancy.EmployerContact?.Phone;
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
                vm.VacancyReferenceNumber = $"VAC{vacancy.VacancyReference}";
                vm.TrainingTitle = programme.Title;
                vm.TrainingType = programme.ApprenticeshipType.GetDisplayName();
                vm.TrainingLevel = programme.Level.GetDisplayName();
                vm.ExpectedDuration = (vacancy.Wage.DurationUnit.HasValue && vacancy.Wage.Duration.HasValue)
                    ? vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value)
                    : null;
                vm.HoursPerWeek = $"{vacancy.Wage.WeeklyHours:0.##}";
                vm.WageInfo = vacancy.Wage.WageAdditionalInformation;
                vm.WageText = vacancy.StartDate.HasValue
                    ? vacancy.Wage.ToText(vacancy.StartDate)
                    : null;
                vm.WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription;
                vm.SubmittedDate = vacancy.SubmittedDate.Value;
                vm.VacancyReviewsApprovedCount = approvedCountTask.Result;
                vm.VacancyReviewsApprovedFirstTimeCount = approvedFirstTimeCountTask.Result;

                vm.FieldIdentifiers = await GetFieldIdentifiersViewModel(review);
                vm.ReviewerComment = review.ManualQaComment;
                vm.ReviewHistories = historiesVm;
                vm.IsResubmission = review.SubmissionCount > 1;

                vm.ReviewerName = review.ReviewedByUser.Name;
                vm.ReviewedDate = review.ReviewedDate.GetValueOrDefault();

                vm.ManualOutcome = review.ManualOutcome;

                if(review.Status == ReviewStatus.Closed)
                {
                    vm.PageTitle = GetPageTitle(historiesVm, review.Id, review.ManualOutcome);
                }

                vm.AutomatedQaResults = GetAutomatedQaResultViewModel(review);
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, "Unable to map vacancy to view model. Unexpected null fields.");
                throw;
            }

            return vm;
        }

        private string GetPageTitle(ReviewHistoriesViewModel historiesVm, Guid reviewId, ManualQaOutcome? reviewManualOutcome)
        {
            var timeFrame = historiesVm.Items.First().ReviewId == reviewId ? "Latest" : "Historical";
            var outcome = reviewManualOutcome.GetValueOrDefault().ToString().ToLower();
            return $"{timeFrame} review -  {outcome} (read only)";
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
            var vm = new List<AutomatedQaResultViewModel>();

            if (review.AutomatedQaOutcomeIndicators == null || review.AutomatedQaOutcome == null)
                return vm;

            var referredOutcomes = review.AutomatedQaOutcomeIndicators
                .Where(i => i.IsReferred)
                .Select(i => i.RuleOutcomeId)
                .ToList();

            foreach (var ruleOutcome in review.AutomatedQaOutcome.RuleOutcomes)
            {
                vm.AddRange(
                ruleOutcome.Details
                    .Where(d => referredOutcomes.Contains(d.Id))
                    .Select(d => new AutomatedQaResultViewModel
                {
                    OutcomeId = d.Id.ToString(),
                    FieldId = d.Target,
                    Checked = true,
                    Text = _ruleTemplateRunner.ToText(ruleOutcome.RuleId, d.Data, FieldDisplayNameResolver.Resolve(d.Target))
                }));
            }

            //sort by the order of the fields on the review page
            return vm.OrderBy(v => ReviewFields.Keys.ToList().FindIndex(k => k == v.FieldId)).ToList();
        }
    }
}