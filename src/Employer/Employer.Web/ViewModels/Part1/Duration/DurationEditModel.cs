using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.DurationValidationMessages;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Duration
{
    public class DurationEditModel : VacancyRouteModel
    {
        [TypeOfInteger(ErrorMessage = ErrMsg.TypeOfInteger.Duration)]
        public string Duration { get; set; }

        public DurationUnit DurationUnit { get; set; }

        public string WorkingWeekDescription { get; set; }

        [TypeOfDecimal(2, ErrorMessage = ErrMsg.TypeOfDecimal.WeeklyHours)]
        public string WeeklyHours { get; set; }
    }
}
