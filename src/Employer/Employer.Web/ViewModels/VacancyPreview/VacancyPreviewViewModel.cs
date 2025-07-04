using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;

public class VacancyPreviewViewModel : DisplayVacancyViewModel
{
    public EntityValidationResult SoftValidationErrors { get; internal set; }
    public bool CanHideValidationSummary { get; internal set; }

    public bool HasWage { get; internal set; }
    public bool HasProgramme { get; internal set; }
    public bool CanShowReference { get; set; }
    public bool CanShowDraftHeader { get; internal set; }
    public string RejectedReason { get; set; }
    public bool? SubmitToEsfa { get; set; }
    public ReviewSummaryViewModel Review { get; set; } = new();
    public string SubmitButtonText => Review.HasBeenReviewed ? "Resubmit advert" : "Submit advert";
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
        nameof(OtherRequirements),
        nameof(EmployerDescription),
        nameof(EmployerName),
        nameof(EmployerWebsiteUrl),
        nameof(EmployerContactName),
        nameof(EmployerContactEmail),
        nameof(EmployerContactTelephone),
        nameof(EmployerAddressElements),
        nameof(ApplicationMethod),
        nameof(ApplicationInstructions),
        nameof(ApplicationUrl),
        nameof(ProviderName),
        nameof(TrainingType),
        nameof(TrainingTitle)
    };

    public int AccountLegalEntityCount { get ; set ; }

    public bool HasSelectedEmployerNameOption => EmployerNameOption != null;

    public ValidationSummaryViewModel ValidationErrors { get; set; } = new ValidationSummaryViewModel();
}

public enum VacancyPreviewSectionState
{
    Incomplete,
    Valid,
    Invalid,
    InvalidIncomplete,
    Review
}