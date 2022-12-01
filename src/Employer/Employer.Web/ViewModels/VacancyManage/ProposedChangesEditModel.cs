using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.DateValidationMessages;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyManage
{
    public class ProposedChangesEditModel : VacancyRouteModel
    {
        [TypeOfDate(ErrorMessage = ErrMsg.TypeOfDate.ClosingDate)]
        public string ProposedClosingDate { get; set; }

        [TypeOfDate(ErrorMessage = ErrMsg.TypeOfDate.StartDate)]
        public string ProposedStartDate { get; set; }
    }
}
