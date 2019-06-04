using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using Microsoft.AspNetCore.Mvc;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.PositionValidationMessages;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions
{
    public class NumberOfPositionsEditModel : VacancyRouteModel
    {
        [TypeOfInteger(ErrorMessage = ErrMsg.TypeOfInteger.NumberOfPositions)]
        public string NumberOfPositions { get; set; }
    }
}
