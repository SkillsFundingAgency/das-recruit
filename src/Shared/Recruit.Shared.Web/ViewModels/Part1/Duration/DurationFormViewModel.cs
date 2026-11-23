using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.DurationValidationMessages;

namespace Esfa.Recruit.Shared.Web.ViewModels.Part1.Duration;

public class DurationFormViewModel
{
    [TypeOfInteger(ErrorMessage = ErrMsg.TypeOfInteger.Duration)]
    public string Duration { get; set; }

    public DurationUnit DurationUnit { get; set; }
    public string WorkingWeekDescription { get; set; }

    [TypeOfDecimal(2, ErrorMessage = ErrMsg.TypeOfDecimal.WeeklyHours)]
    public string WeeklyHours { get; set; }

    public string TrainingTitle { get; set; }
    public int TrainingDurationMonths { get; set; }
    public string CharacterCountMessage { get; set; }

    public bool ShowTraining => string.IsNullOrWhiteSpace(TrainingTitle) == false && TrainingDurationMonths > 0;
}
