using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsEditModel : QualificationEditModel
    {        
        public List<QualificationEditModel> Qualifications { get; set; }

        public string AddQualificationAction { get; set; }
        public string RemoveQualification { get; set; }

        public bool IsAddingQualification => !string.IsNullOrWhiteSpace(AddQualificationAction);

        public bool IsRemovingQualification => !string.IsNullOrEmpty(RemoveQualification);
    }

    public class QualificationEditModel : VacancyRouteModel
    {
        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public QualificationWeighting? Weighting { get; set; }
    }
}
