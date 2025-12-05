using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;

public class VacancyPreviewViewModel : DisplayVacancyViewModel 
{
    public EntityValidationResult SoftValidationErrors { get; internal set; }
    public bool CanHideValidationSummary { get; internal set; }

    public bool HasWage { get; internal set; }
    public bool HasProgramme => !string.IsNullOrEmpty(TrainingTitle);
    public bool CanShowReference { get; set; }
    public bool CanShowDraftHeader { get; internal set; }
    public string InfoMessage { get; internal set; }
    public bool RequiresEmployerReview { get; internal set; }
    public bool HasSoftValidationErrors => SoftValidationErrors?.HasErrors == true;
    public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
    public ValidationSummaryViewModel ValidationErrors { get; set; } = new ValidationSummaryViewModel(); 
        
    public string SubmitButtonText => RequiresEmployerReview
        ? Status == VacancyStatus.Rejected
            ? "Resubmit vacancy to employer"
            : "Send to employer"
        : Review.HasBeenReviewed
            ? "Resubmit vacancy"
            : "Submit vacancy";

    [Required(ErrorMessage = "You must confirm that the information is correct before submitting it.")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm that the information is correct before submitting it.")]
    public bool HasUserConfirmation { get; set; }
    public int AdditionalQuestionCount { get; set; }

    public IList<string> OrderedFieldNames => new List<string>
    {
        nameof(ShortDescription),
        nameof(ClosingDate),
        nameof(WorkingWeekDescription),
        nameof(HasWage),
        nameof(WorkingWeekDescription),
        nameof(HoursPerWeek),
        nameof(WageText),
        nameof(WageInfo),
        nameof(CompanyBenefitsInformation),
        nameof(ExpectedDuration),
        nameof(PossibleStartDate),
        nameof(HasProgramme),
        nameof(TrainingLevel),
        nameof(NumberOfPositions),
        nameof(VacancyDescription),
        nameof(TrainingDescription),
        nameof(AdditionalTrainingDescription),
        nameof(OutcomeDescription),
        nameof(Skills),
        nameof(Qualifications),
        nameof(ThingsToConsider),
        nameof(EmployerDescription),
        nameof(EmployerName),
        nameof(EmployerWebsiteUrl),
        nameof(EmployerAddressElements),
        nameof(ApplicationMethod),
        nameof(ApplicationInstructions),
        nameof(ApplicationUrl),
        nameof(ProviderName),
        nameof(ProviderContactName),
        nameof(ProviderContactEmail),
        nameof(ProviderContactTelephone),
        nameof(TrainingType),
        nameof(TrainingTitle)
    };

    public int AccountCount { get; set; }
    public int AccountLegalEntityCount { get ; set ; }
    public string AccountId { get; set; }
    public bool CanShowVacancyClonedStatusHeader => !string.IsNullOrEmpty(VacancyClonedInfoMessage);
    public string VacancyClonedInfoMessage { get; set; }
    public bool ShowReviewVacancyAsEmployerHasChangedBanner { get; set; }
}

public enum VacancyPreviewSectionState
{
    Incomplete,
    Valid,
    Invalid,
    InvalidIncomplete,
    Review
}