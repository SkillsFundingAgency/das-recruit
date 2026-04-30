using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.ArchiveVacancy;

public class ArchiveEditModel : VacancyRouteModel
{
    [Required(ErrorMessage = ValMsg.ValidationMessages.ArchiveVacancyConfirmationMessages.SelectionRequired)]
    public bool? ConfirmArchive { get; set; }
        
    public VacancyStatus Status { get; set; }
}