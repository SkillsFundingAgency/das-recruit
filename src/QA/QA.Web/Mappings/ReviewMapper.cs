using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.Services;
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
        private readonly ILogger<ReviewMapper> _logger;
        private readonly IQaVacancyClient _vacancyClient;
        private readonly Lazy<IList<string>> _qualifications;
        private readonly IRuleMessageTemplateRunner _ruleTemplateRunner;
        private readonly IReviewSummaryService _reviewSummaryService;

        public ReviewMapper(ILogger<ReviewMapper> logger,
                    IQaVacancyClient vacancyClient,
                    IRuleMessageTemplateRunner ruleTemplateRunner,
                    IReviewSummaryService reviewSummaryService)
        {
            _logger = logger;
            _vacancyClient = vacancyClient;
            _qualifications = new Lazy<IList<string>>(() => _vacancyClient.GetCandidateQualificationsAsync().Result.QualificationTypes);
            _ruleTemplateRunner = ruleTemplateRunner;
            _reviewSummaryService = reviewSummaryService;
        }

        private static readonly Dictionary<string, IEnumerable<string>> ReviewFields = new Dictionary<string, IEnumerable<string>>
        {
            //These need to be in display order
            { FieldIdResolver.ToFieldId(v => v.EmployerAccountId), new string[0]},
            { FieldIdResolver.ToFieldId(v => v.Title), new[]{FieldIdentifiers.Title} },
            { FieldIdResolver.ToFieldId(v => v.EmployerName), new []{FieldIdentifiers.EmployerName} },
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
            { FieldIdResolver.ToFieldId(v => v.AnonymousReason), new [] {FieldIdentifiers.EmployerName }},
            { FieldIdResolver.ToFieldId(v => v.DisabilityConfident), new []{FieldIdentifiers.DisabilityConfident} },
            { FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl), new []{FieldIdentifiers.EmployerWebsiteUrl} },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1), new []{FieldIdentifiers.EmployerAddress} },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2), new []{FieldIdentifiers.EmployerAddress} },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3), new []{FieldIdentifiers.EmployerAddress} },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4), new []{ FieldIdentifiers.EmployerAddress} },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode), new[]{FieldIdentifiers.EmployerAddress}},
            { FieldIdResolver.ToFieldId(v => v.EmployerLocations), new[]{FieldIdentifiers.EmployerAddress}},
            { FieldIdResolver.ToFieldId(v => v.EmployerLocationInformation), new[]{FieldIdentifiers.EmployerAddress}},
            { FieldIdResolver.ToFieldId(v => v.EmployerContact.Name), new []{ FieldIdentifiers.EmployerContact }},
            { FieldIdResolver.ToFieldId(v => v.EmployerContact.Email), new []{FieldIdentifiers.EmployerContact} },
            { FieldIdResolver.ToFieldId(v => v.EmployerContact.Phone), new []{FieldIdentifiers.EmployerContact }},
            { FieldIdResolver.ToFieldId(v => v.TrainingProvider.Ukprn) , new []{FieldIdentifiers.Provider} },
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Name), new []{FieldIdentifiers.ProviderContact}},
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Email), new []{FieldIdentifiers.ProviderContact}},
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Phone), new []{FieldIdentifiers.ProviderContact}},
            { FieldIdResolver.ToFieldId(v => v.ApplicationInstructions), new [] {FieldIdentifiers.ApplicationInstructions }},
            { FieldIdResolver.ToFieldId(v => v.ApplicationMethod), new [] {FieldIdentifiers.ApplicationMethod} },
            { FieldIdResolver.ToFieldId(v => v.ApplicationUrl), new []{FieldIdentifiers.ApplicationUrl} },
            { FieldIdResolver.ToFieldId(v => v.AdditionalQuestion1), new []{FieldIdentifiers.AdditionalQuestion1} },
            { FieldIdResolver.ToFieldId(v => v.AdditionalQuestion2), new []{FieldIdentifiers.AdditionalQuestion2} }
        };

        private static List<FieldIdentifierViewModel> GetFieldIndicators(VacancyType vacancyType)
        {
            if (vacancyType == VacancyType.Apprenticeship)
            {
                return new List<FieldIdentifierViewModel>
                {
                    //These need to be in display order
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Title, Text = "Title" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ShortDescription, Text = "Brief overview" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ClosingDate, Text = "Closing date" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.WorkingWeek, Text = "Working week" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Wage, Text = "Annual wage" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.CompanyBenefitsInformation, Text = "Company Benefits" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.ExpectedDuration, Text = "Expected duration" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.PossibleStartDate, Text = "Possible start" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.TrainingLevel, Text = "Apprenticeship level" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.NumberOfPositions, Text = "Positions" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.VacancyDescription, Text = "What will the apprentice be doing?" },
                    new FieldIdentifierViewModel
                    {
                        FieldIdentifier = FieldIdentifiers.TrainingDescription,
                        Text = "What training will the apprentice take and what qualification will the apprentice get at the end?"
                    },
                    new FieldIdentifierViewModel
                    {
                        FieldIdentifier = FieldIdentifiers.AdditionalTrainingDescription,
                        Text = "Additional training information (optional)"
                    },
                    new FieldIdentifierViewModel
                    {
                        FieldIdentifier = FieldIdentifiers.OutcomeDescription,
                        Text = "What is the expected career progression after this apprenticeship?"
                    },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Skills, Text = "Skills" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Qualifications, Text = "Qualifications" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.ThingsToConsider, Text = "Things to consider" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.EmployerDescription, Text = "Employer description" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.EmployerName, Text = "Employer name" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.DisabilityConfident, Text = "Disability confident" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.EmployerWebsiteUrl, Text = "Employer website" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.EmployerContact, Text = "Contact details" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.EmployerAddress, Text = "Employer address" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Provider, Text = "Training provider" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ProviderContact, Text = "Contact details" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Training, Text = "Training" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.ApplicationMethod, Text = "Application method" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.ApplicationUrl, Text = "Apply now web address" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.ApplicationInstructions, Text = "Application process" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.AdditionalQuestion1, Text = "Additional Question 1" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.AdditionalQuestion2, Text = "Additional Question 2" }
                };
            }
            else
            {
                return new List<FieldIdentifierViewModel>
                {
                    //These need to be in display order
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Title, Text = "Vacancy title" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ShortDescription, Text = "Summary of the traineeship" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ClosingDate, Text = "Closing date" },
                    // summary section
                    new FieldIdentifierViewModel
                    {
                        FieldIdentifier = FieldIdentifiers.TrainingDescription,
                        Text = "Training provided"
                    },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.WorkingWeek, Text = "Working week details" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.ExpectedDuration, Text = "Duration" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.PossibleStartDate, Text = "Traineeship start date" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.NumberOfPositions, Text = "Number of positions" },
                    // requirements and prospects section
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Skills, Text = "Skills" },
                    new FieldIdentifierViewModel
                    {
                        FieldIdentifier = FieldIdentifiers.OutcomeDescription,
                        Text = "Future prospects"
                    },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.ThingsToConsider, Text = "Other things to consider" },
                    // about the employer
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.EmployerDescription, Text = "Employer information" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.EmployerName, Text = "Employer name" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.DisabilityConfident, Text = "Disability Confident employer?" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.EmployerWebsiteUrl, Text = "Organisation website" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.EmployerContact, Text = "Contact details" },
                    new FieldIdentifierViewModel
                        { FieldIdentifier = FieldIdentifiers.EmployerAddress, Text = "Work experience address" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.Provider, Text = "Training provider" },
                    new FieldIdentifierViewModel { FieldIdentifier = FieldIdentifiers.ProviderContact, Text = "Contact details" }
                };
            }
        }

        public async Task<ReviewViewModel> Map(VacancyReview review)
        {
            var vacancy = review.VacancySnapshot;

            var currentVacancy = _vacancyClient.GetVacancyAsync(review.VacancyReference);

            var programmeTask = vacancy.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship 
                ? _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId)
                : Task.FromResult((IApprenticeshipProgramme)null);
            
            var reviewHistoryTask = _vacancyClient.GetVacancyReviewHistoryAsync(review.VacancyReference);

            var approvedCountTask = _vacancyClient.GetApprovedCountAsync(vacancy.SubmittedByUser.UserId);

            var approvedFirstTimeCountTask = _vacancyClient.GetApprovedFirstTimeCountAsync(vacancy.SubmittedByUser.UserId);

            var reviewSummaryTask = _reviewSummaryService.GetReviewSummaryViewModelAsync(review.Id,
                    ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators());

            var anonymousApprovedCountTask = vacancy.IsAnonymous ? _vacancyClient.GetAnonymousApprovedCountAsync(vacancy.AccountLegalEntityPublicHashedId) : Task.FromResult(0);

            await Task.WhenAll(
                currentVacancy,
                programmeTask,
                approvedCountTask,
                approvedFirstTimeCountTask,
                reviewHistoryTask,
                reviewSummaryTask,
                anonymousApprovedCountTask);

            var programme = programmeTask.Result;
            var currentVacancyResult = currentVacancy.Result;
            var historiesVm = GetReviewHistoriesViewModel(reviewHistoryTask.Result);

            var vm = new ReviewViewModel();
            vm.Review = reviewSummaryTask.Result;
            try
            {
                vm.SubmittedByName = vacancy.SubmittedByUser.Name;
                vm.SubmittedByEmail = vacancy.SubmittedByUser.Email;
                vm.ApplicationInstructions = vacancy.ApplicationInstructions;
                vm.ApplicationMethod = vacancy.ApplicationMethod.Value;
                vm.ApplicationUrl = vacancy.ApplicationUrl;
                vm.ClosingDate = vacancy.ClosingDate?.AsGdsDate();
                vm.EducationLevelName = vacancy.VacancyType == VacancyType.Apprenticeship 
                    ? EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel) 
                    : "";
                vm.EmployerContactName = vacancy.EmployerContact?.Name;
                vm.EmployerContactEmail = vacancy.EmployerContact?.Email;
                vm.EmployerContactTelephone = vacancy.EmployerContact?.Phone;
                vm.EmployerDescription = vacancy.EmployerDescription;
                vm.EmployerName = vacancy.EmployerName;
                vm.EmployerNameOption = vacancy.EmployerNameOption.Value;
                vm.AnonymousReason = vacancy.AnonymousReason;
                vm.AnonymousApprovedCount = anonymousApprovedCountTask.Result;
                vm.EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl;
                if (vacancy.EmployerLocation != null)
                {
                    vm.EmployerAddressElements = vacancy.EmployerAddressForDisplay();
                }
                vm.EmployerLocationOption = vacancy.EmployerLocationOption;
                vm.EmployerLocationInformation = vacancy.EmployerLocationInformation;
                vm.EmployerLocations = vacancy.EmployerLocations;
                vm.LegalEntityName = vacancy.LegalEntityName;
                vm.AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId;
                vm.IsDisabilityConfident = vacancy.IsDisabilityConfident;
                vm.NumberOfPositionsCaption = $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available";
                vm.OutcomeDescription = vacancy.OutcomeDescription;
                vm.PossibleStartDate = vacancy.StartDate?.AsGdsDate();
                vm.ProviderContactName = vacancy.ProviderContact?.Name;
                vm.ProviderContactEmail = vacancy.ProviderContact?.Email;
                vm.ProviderContactTelephone= vacancy.ProviderContact?.Phone;
                vm.ProviderName = vacancy.TrainingProvider.Name;
                vm.Qualifications = vacancy.Qualifications.SortQualifications(_qualifications.Value).AsText();
                vm.ShortDescription = vacancy.ShortDescription;
                vm.Skills = vacancy.Skills ?? Enumerable.Empty<string>();
                vm.OwnerType = vacancy.OwnerType;
                vm.ThingsToConsider = vacancy.ThingsToConsider;
                vm.Title = vacancy.Title;
                vm.TrainingDescription = vacancy.TrainingDescription;
                vm.AdditionalTrainingDescription = vacancy.AdditionalTrainingDescription;
                vm.VacancyDescription = vacancy.Description;
                vm.VacancyReferenceNumber = $"VAC{vacancy.VacancyReference}";
                if (programme != null)
                {
                    vm.TrainingTitle = programme.Title;
                    vm.TrainingType = programme.ApprenticeshipType.GetDisplayName();
                    vm.TrainingLevel = programme.ApprenticeshipLevel.GetDisplayName();
                    vm.Level = programme.ApprenticeshipLevel;
                }
                
                if (vacancy.Wage != null)
                {
                    vm.ExpectedDuration = (vacancy.Wage.DurationUnit.HasValue && vacancy.Wage.Duration.HasValue)
                        ? vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value)
                        : null;
                    vm.HoursPerWeek = $"{vacancy.Wage.WeeklyHours:0.##}";
                    vm.WageInfo = vacancy.Wage.WageAdditionalInformation;
                    vm.WageText = vacancy.StartDate.HasValue
                        ? vacancy.Wage.ToText(vacancy.StartDate)
                        : null;
                    vm.WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription;
                    vm.CompanyBenefitsInformation = vacancy.Wage.CompanyBenefitsInformation;
                }
                vm.VacancyType = vacancy.VacancyType;
                
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

                if (review.Status == ReviewStatus.Closed)
                {
                    vm.PageTitle = GetPageTitle(historiesVm, review.Id, review.ManualOutcome, currentVacancyResult);
                }

                vm.AutomatedQaResults = GetAutomatedQaResultViewModel(review);
                vm.IsVacancyDeleted = currentVacancyResult.IsDeleted;
                vm.AdditionalQuestion1 = vacancy.AdditionalQuestion1;
                vm.AdditionalQuestion2 = vacancy.AdditionalQuestion2;
                vm.HasAdditionalQuestions = vacancy.HasSubmittedAdditionalQuestions;
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, "Unable to map vacancy to view model. Unexpected null fields.");
                throw;
            }

            return vm;
        }

        private string GetPageTitle(ReviewHistoriesViewModel historiesVm, Guid reviewId, ManualQaOutcome? reviewManualOutcome,Vacancy vacancy)
        {
            var outcome = reviewManualOutcome.GetValueOrDefault().ToString().ToLower();
            bool hasReviews = historiesVm.Items.First().ReviewId == reviewId;
            if (vacancy.IsDeleted && vacancy.Status == VacancyStatus.Referred)
            {
                var timeFrame = hasReviews ? "Deleted vacancy" : "Historical review";
                return $"{timeFrame} -  {outcome} (read only)";
            }
            else
            {
                var timeFrame = hasReviews ? "Latest" : "Historical";
                return $"{timeFrame} review -  {outcome} (read only)";
            }
        }

        private async Task<IEnumerable<FieldIdentifierViewModel>> GetFieldIdentifiersViewModel(VacancyReview currentReview)
        {
            var vm = GetFieldIndicators(currentReview.VacancySnapshot.VacancyType.GetValueOrDefault());

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

        public List<ManualQaFieldIndicator> GetManualQaFieldIndicators(ReviewEditModel m, VacancyType vacancyType)
        {
            return GetFieldIndicators(vacancyType).Select(f => new ManualQaFieldIndicator
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
                    Checked = !review.DismissedAutomatedQaOutcomeIndicators?.Contains(d.Target.ToString()) ?? true,
                    Text = _ruleTemplateRunner.ToText(ruleOutcome.RuleId, d.Data, FieldDisplayNameResolver.Resolve(d.Target))
                }));
            }

            //sort by the order of the fields on the review page
            return vm.OrderBy(v => ReviewFields.Keys.ToList().FindIndex(k => k == v.FieldId)).ToList();
        }
    }
}