﻿using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.DeleteVacancy
{
    public class DeleteEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValMsg.ValidationMessages.DeleteVacancyConfirmationMessages.SelectionRequired)]
        public bool? ConfirmDeletion { get; set; }
    }
}
