using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsEditModel : QualificationEditModel
    {
        // Cannot inherit EmployerAccountId and VacancyId from VacancyRouteModel because QualificationEditModel cannot set these properties when it is being reformed from TempData.
        private string _employerAccountId;

        [FromRoute]
        public string EmployerAccountId
        {
            get { return _employerAccountId; }
            set { _employerAccountId = value.ToUpper(); }
        }

        [FromRoute]
        public Guid VacancyId { get; set; }

        public List<QualificationEditModel> Qualifications { get; set; }

        public string AddQualificationAction { get; set; }
        public string RemoveQualification { get; set; }

        public bool IsAddingQualification => !string.IsNullOrWhiteSpace(AddQualificationAction);

        public bool IsRemovingQualification => !string.IsNullOrEmpty(RemoveQualification);
    }

    public class QualificationEditModel
    {
        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public QualificationWeighting? Weighting { get; set; }
    }
}
