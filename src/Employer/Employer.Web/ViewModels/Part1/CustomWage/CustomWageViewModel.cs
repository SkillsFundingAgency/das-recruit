using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.WageValidationMessages;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.CustomWage
{
    public class CustomWageViewModel : VacancyRouteModel
    {
        public WageType? WageType { get; set; }

        [TypeOfMoney(ErrorMessage = ErrMsg.TypeOfMoney.FixedWageYearlyAmount)]
        public string FixedWageYearlyAmount { get; set; }

        public string WageAdditionalInformation { get; set; }

        public string MinimumWageStartFrom { get; set; }
        public string NationalMinimumWageLowerBoundHourly { get; set; }
        public string NationalMinimumWageUpperBoundHourly { get; set; }
        public string NationalMinimumWageYearly { get; set; }
        public string ApprenticeshipMinimumWageHourly { get; set; }
        public string ApprenticeshipMinimumWageYearly { get; set; }
        public decimal WeeklyHours { get; set; }

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(WageType),
            nameof(FixedWageYearlyAmount),
            nameof(WageAdditionalInformation)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }
    }
}
