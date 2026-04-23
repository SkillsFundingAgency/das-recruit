using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.ArchiveVacancy;

public class ArchiveEditModel : VacancyRouteModel
{
    [Required(ErrorMessage = ValMsg.ValidationMessages.ArchiveVacancyConfirmationMessages.SelectionRequired)]
    public bool? ConfirmArchive { get; set; }
        
    public VacancyStatus Status { get; set; }
}