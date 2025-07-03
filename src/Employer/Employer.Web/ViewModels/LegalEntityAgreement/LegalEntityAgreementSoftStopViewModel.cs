using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.LegalEntityAgreement;

public class LegalEntityAgreementSoftStopViewModel : VacancyRouteModel
{
    public bool HasLegalEntityAgreement { get; set; }
    public string LegalEntityName { get; set; }
    public bool IsTaskListComplete { get; set; }
}