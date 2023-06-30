using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public class VacancySummaryViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public long? VacancyReference { get; set; }
        public string EmployerName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public VacancyStatus Status { get; set; }
        public DateTime? ClosingDate { get; set; }
        public ApplicationMethod? ApplicationMethod { get; set; }
        public string ProgrammeId { get; set; }
        public string TrainingTitle { get; set; }
        public TrainingType TrainingType { get; set; }
        public ApprenticeshipLevel TrainingLevel { get; set; }
        public bool IsTransferred { get; set; }
        public int NoOfNewApplications { get; set; }
        public int NoOfSuccessfulApplications { get; set; }
        public int NoOfUnsuccessfulApplications { get; set; }
        public int NoOfSharedApplications { get; set; }
        public int NoOfAllSharedApplications { get; set; }

        public bool HasVacancyReference => VacancyReference.HasValue;
        public bool HasNoVacancyReference => !HasVacancyReference;
        public bool CanShowVacancyApplicationsCount => (Status== VacancyStatus.Live || Status == VacancyStatus.Closed) 
                                             && (ApplicationMethod == Vacancies.Client.Domain.Entities.ApplicationMethod.ThroughFindAnApprenticeship || ApplicationMethod == Vacancies.Client.Domain.Entities.ApplicationMethod.ThroughFindATraineeship);

        public bool HasApplications => NoOfApplications > 0;
        public bool HasNoApplications => !HasApplications;
        public bool HasNewApplications => NoOfNewApplications > 0;
        public bool HasSharedApplications => NoOfAllSharedApplications > 0;
        public bool HasNoSharedApplications => !HasSharedApplications;
        public bool HasNewSharedApplications => NoOfSharedApplications > 0;
        public bool IsLive => Status == VacancyStatus.Live;
        public bool IsNotLive => !IsLive;

        public bool IsSubmittable => Status == VacancyStatus.Draft || Status == VacancyStatus.Referred || Status == VacancyStatus.Rejected;
        public bool IsNotSubmittable => !IsSubmittable;
        public int NoOfApplications => NoOfNewApplications + NoOfSuccessfulApplications + NoOfUnsuccessfulApplications;
        public bool IsTaskListCompleted { get; set; }
    }
}
