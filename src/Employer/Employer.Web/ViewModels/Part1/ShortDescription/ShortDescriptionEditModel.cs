using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using ErrMsg = Esfa.Recruit.Employer.Web.ViewModels.ValidationMessages.ShortDescriptionValidationMessages;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription
{
    public class ShortDescriptionEditModel : VacancyRouteModel
    {
        [TypeOfInteger(ErrorMessage = ErrMsg.TypeOfInteger.NumberOfPositions)]
        public string NumberOfPositions { get; set; }

        public string ShortDescription { get; set; }
    }
}
