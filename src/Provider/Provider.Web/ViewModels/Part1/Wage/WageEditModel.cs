using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.WageValidationMessages;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage
{
    public class WageEditModel : VacancyRouteModel
    {
        [TypeOfInteger(ErrorMessage = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.DurationValidationMessages.TypeOfInteger.Duration)]
        public string Duration { get; set; }

        public DurationUnit DurationUnit { get; set; }

        public string WorkingWeekDescription { get; set; }

        [TypeOfDecimal(2, ErrorMessage = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.DurationValidationMessages.TypeOfDecimal.WeeklyHours)]
        public string WeeklyHours { get; set; }

        public WageType? WageType { get; set; }

        [TypeOfMoney(ErrorMessage = ErrMsg.TypeOfMoney.FixedWageYearlyAmount)]
        public string FixedWageYearlyAmount { get; set; }

        public string WageAdditionalInformation { get; set; }
    }
}