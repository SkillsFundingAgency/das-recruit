using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.WageValidationMessages;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage
{
    public class WageEditModel : VacancyRouteModel
    {
        public WageType? WageType { get; set; }

        [TypeOfMoney(ErrorMessage = ErrMsg.TypeOfMoney.FixedWageYearlyAmount)]
        public string FixedWageYearlyAmount { get; set; }

        public string WageAdditionalInformation { get; set; }
        public string CompanyBenefitsInformation { get; set; }
    }
}