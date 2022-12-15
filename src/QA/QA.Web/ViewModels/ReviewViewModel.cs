using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using FieldIdentifier = Esfa.Recruit.Shared.Web.Mappers.FieldIdentifiers;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class 
        ReviewViewModel : ReviewEditModel
    {
        private const string CssFieldChanged = "field-changed";
        
        public string Title { get; internal set; }
        public string EmployerName { get; internal set; }
        public string ClosingDate { get; internal set; }
        public string ApplicationInstructions { get; internal set; }
        public ApplicationMethod ApplicationMethod { get; internal set; }
        public string ApplicationUrl { get; internal set; }
        public string EducationLevelName { get; internal set; }
        public string EmployerContactName { get; internal set; }
        public string EmployerContactEmail { get; internal set; }
        public string EmployerContactTelephone { get; internal set; }
        public string EmployerDescription { get; internal set; }
        public EmployerNameOption EmployerNameOption { get; internal set; }
        public string AnonymousReason { get; internal set; }
        public int AnonymousApprovedCount { get; internal set; }
        public string EmployerWebsiteUrl { get; internal set; }
        public IEnumerable<string> EmployerAddressElements { get; internal set; }
        public bool IsDisabilityConfident { get; set; }
        public string LegalEntityName { get; set; }
        public string NumberOfPositionsCaption { get; internal set; }
        public string HoursPerWeek { get; internal set; }
        public OwnerType OwnerType { get; internal set; }
        public string PossibleStartDate { get; internal set; }
        public string ProviderContactName { get; internal set; }
        public string ProviderContactEmail { get; internal set; }
        public string ProviderContactTelephone { get; internal set; }
        public string ProviderName { get; internal set; }
        public string ThingsToConsider { get; internal set; }
        public string VacancyReferenceNumber { get; internal set; }
        public string TrainingTitle { get; internal set; }
        public string TrainingType { get; internal set; }
        public string TrainingLevel { get; internal set; }
        public string TraineeRoute { get; internal set; }
        public string ExpectedDuration { get; internal set; }
        public string WageInfo { get; internal set; }
        public string MapUrl { get; internal set; }
        public IEnumerable<string> Qualifications { get; internal set; }
        public IEnumerable<string> Skills { get; internal set; }
        public string WageText { get; internal set; }
        public string SubmittedByName { get; internal set; }
        public string SubmittedByEmail { get; internal set; }
        public int VacancyReviewsApprovedCount { get; internal set; }
        public int VacancyReviewsApprovedFirstTimeCount { get; internal set; }
        public DateTime SubmittedDate { get; internal set; }
        public ReviewHistoriesViewModel ReviewHistories { get; internal set; }
        public IEnumerable<FieldIdentifierViewModel> FieldIdentifiers { get; set; }
        public IEnumerable<AutomatedQaResultViewModel> AutomatedQaResults { get; set; }
        public bool IsResubmission { get; set; }
        public string ReviewerName { get; set; }
        public DateTime ReviewedDate { get; set; }
        public ReviewSummaryViewModel Review { get; set; }
        public ManualQaOutcome? ManualOutcome { get; set; }
        public string AdditionalQuestion1 { get; internal set; }
        public string AdditionalQuestion2 { get; internal set; }
        public bool HasAdditionalQuestions { get; internal set; }

        public bool IsAnonymous => EmployerNameOption == EmployerNameOption.Anonymous;
        public bool IsApproved => ManualOutcome.GetValueOrDefault() == ManualQaOutcome.Approved;
        public string ReviewedDateDay => ReviewedDate.ToUkTime().AsGdsDate();
        public string ReviewedDateTime => ReviewedDate.ToUkTime().AsGdsTime();
        public bool IsFirstSubmission => IsResubmission == false;
        public bool HasChangedFields => FieldIdentifiers.Any(f => f.FieldValueHasChanged);
        public bool HasSpecifiedThroughFaaApplicationMethod => ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
        public bool HasSpecifiedThroughExternalApplicationSiteMethod => ApplicationMethod == ApplicationMethod.ThroughExternalApplicationSite;
        public bool HasApplicationInstructions => string.IsNullOrEmpty(ApplicationInstructions) == false;
        public bool HasApplicationUrl => string.IsNullOrEmpty(ApplicationUrl) == false;
        public bool HasPreviouslySubmitted => VacancyReviewsApprovedCount > 0;
        public bool HasNotPreviouslySubmitted => HasPreviouslySubmitted == false;
        public bool HasOneAnonymousApproved => AnonymousApprovedCount == 1;
        public string SubmittedDateTime => SubmittedDate.ToUkTime().AsGdsDateTime();
        public bool IsNotDisabilityConfident => IsDisabilityConfident == false;
        public bool IsEmployerVacancy => OwnerType == OwnerType.Employer;
        public bool IsProviderVacancy => OwnerType == OwnerType.Provider;
        public string Owner => IsEmployerVacancy ? "employer" : "provider";        
        public string ApplicationInstructionsClass => GetFieldIdentifierCssClass(FieldIdentifier.ApplicationInstructions);
        public string ApplicationMethodClass => GetFieldIdentifierCssClass(FieldIdentifier.ApplicationMethod);
        public string ApplicationUrlClass => GetFieldIdentifierCssClass(FieldIdentifier.ApplicationUrl);
        public string ClosingDateClass => GetFieldIdentifierCssClass(FieldIdentifier.ClosingDate);
        public string EmployerContactClass => GetFieldIdentifierCssClass(FieldIdentifier.EmployerContact);
        public string DisabilityConfidentClass => GetFieldIdentifierCssClass(FieldIdentifier.DisabilityConfident);
        public string EmployerNameClass => GetFieldIdentifierCssClass(FieldIdentifier.EmployerName);
        public string EmployerAddressClass => GetFieldIdentifierCssClass(FieldIdentifier.EmployerAddress);
        public string EmployerDescriptionClass => GetFieldIdentifierCssClass(FieldIdentifier.EmployerDescription);
        public string EmployerWebsiteUrlClass => GetFieldIdentifierCssClass(FieldIdentifier.EmployerWebsiteUrl);
        public string ExpectedDurationClass => GetFieldIdentifierCssClass(FieldIdentifier.ExpectedDuration);
        public string NumberOfPositionsClass => GetFieldIdentifierCssClass(FieldIdentifier.NumberOfPositions);
        public string OutcomeDescriptionClass => GetFieldIdentifierCssClass(FieldIdentifier.OutcomeDescription);
        public string PossibleStartDateClass => GetFieldIdentifierCssClass(FieldIdentifier.PossibleStartDate);
        public string ProviderClass => GetFieldIdentifierCssClass(FieldIdentifier.Provider);
        public string ProviderContactClass => GetFieldIdentifierCssClass(FieldIdentifier.ProviderContact);
        public string QualificationsClass => GetFieldIdentifierCssClass(FieldIdentifier.Qualifications);
        public string SkillsClass => GetFieldIdentifierCssClass(FieldIdentifier.Skills);
        public string ShortDescriptionClass => GetFieldIdentifierCssClass(FieldIdentifier.ShortDescription);
        public string ThingsToConsiderClass => GetFieldIdentifierCssClass(FieldIdentifier.ThingsToConsider);
        public string TitleClass => GetFieldIdentifierCssClass(FieldIdentifier.Title);
        public string TrainingClass => GetFieldIdentifierCssClass(FieldIdentifier.Training);
        public string TrainingDescriptionClass => GetFieldIdentifierCssClass(FieldIdentifier.TrainingDescription);
        public string TrainingLevelClass => GetFieldIdentifierCssClass(FieldIdentifier.TrainingLevel);
        public string TraineeRouteClass => GetFieldIdentifierCssClass(FieldIdentifier.TraineeRoute);
        public string VacancyDescriptionClass => GetFieldIdentifierCssClass(FieldIdentifier.VacancyDescription);
        public string WageClass => GetFieldIdentifierCssClass(FieldIdentifier.Wage);
        public string WorkingWeekClass => GetFieldIdentifierCssClass(FieldIdentifier.WorkingWeek);
        public bool HasAutomatedQaResults => AutomatedQaResults.Any();
        public string PageTitle { get; set; }        
        public bool IsVacancyDeleted { get; set; }
        private string GetFieldIdentifierCssClass(string fieldIdentifer)
        {
            return FieldIdentifiers.Single(f => f.FieldIdentifier == fieldIdentifer).FieldValueHasChanged ? CssFieldChanged : null;
        }
        public ApprenticeshipLevel Level { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string WorkExperienceClass => GetFieldIdentifierCssClass(FieldIdentifier.WorkExperience);
        public VacancyType? VacancyType { get; set; }
    }
}