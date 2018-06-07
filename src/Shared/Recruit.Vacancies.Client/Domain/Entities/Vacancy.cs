using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Vacancy
    {
        public Guid Id { get; internal set; }
        public string EmployerAccountId { get; internal set; }
        public long? VacancyReference { get; internal set; }
        public VacancyStatus Status { get; internal set; }
        public SourceOrigin SourceOrigin { get; internal set; }
        public SourceType SourceType { get; internal set; }
        public long? SourceVacancyReference { get; internal set; }

        public DateTime? ClosedDate { get; internal set; }

        public DateTime? CreatedDate { get; internal set; }
        public VacancyUser CreatedByUser { get; internal set; }

        public DateTime? SubmittedDate { get; internal set; }
        public VacancyUser SubmittedByUser { get; internal set; }

        public DateTime? ApprovedDate { get; internal set; }

        public DateTime? LiveDate { get; internal set; }

        public DateTime? LastUpdatedDate { get; internal set; }
        public VacancyUser LastUpdatedByUser { get; internal set; }
        
        public bool IsDeleted { get; internal set; }
        public DateTime? DeletedDate { get; internal set; }
        public VacancyUser DeletedByUser { get; internal set; }
        
        public string ApplicationInstructions { get; set; }
        public ApplicationMethod? ApplicationMethod { get; set; }
        public string ApplicationUrl { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string Description { get; set; }
        public string EmployerContactEmail { get; set; }
        public string EmployerContactName { get; set; }
        public string EmployerContactPhone { get; set; }
        public string EmployerDescription { get; set; }
        public Address EmployerLocation { get; set; }
        public string EmployerName { get; set; }
        public string EmployerWebsiteUrl { get; set; }
        public int? NumberOfPositions { get; set; }
        public string OutcomeDescription { get; set; }
        public string ProgrammeId { get; set; }
        public List<Qualification> Qualifications { get; set; }
        public string ShortDescription { get; set; }
        public List<string> Skills { get; set; }
        public DateTime? StartDate { get; set; }
        public string ThingsToConsider { get; set; }
        public string Title { get; set; }
        public string TrainingDescription { get; set; }
        public TrainingProvider TrainingProvider { get; set; }
        public Wage Wage { get; set; }

        /// <summary>
        /// We can only delete draft vacancies that have not been deleted
        /// </summary>
        public bool CanDelete => Status == VacancyStatus.Draft && IsDeleted == false;

        /// <summary>
        /// We can only edit draft vacancies that have not been deleted
        /// </summary>
        public bool CanEdit => Status == VacancyStatus.Draft && IsDeleted == false;

        /// <summary>
        /// We can only submit draft vacancies that have not been deleted
        /// </summary>
        public bool CanSubmit => Status == VacancyStatus.Draft && IsDeleted == false;

        /// <summary>
        /// We can only approve submitted vacancies that have not been deleted
        /// </summary>
        public bool CanApprove => (Status == VacancyStatus.PendingReview || Status == VacancyStatus.UnderReview) && IsDeleted == false;

        /// <summary>
        /// We can only refer pending review vacancies that have not been deleted
        /// </summary>
        public bool CanRefer => Status == VacancyStatus.PendingReview && IsDeleted == false;

        /// <summary>
        /// We can only make approved vacancies live that have not been deleted
        /// </summary>
        public bool CanMakeLive => Status == VacancyStatus.Approved && IsDeleted == false;

        /// <summary>
        /// We can send for review vacancies that are submitted and that have not been deleted
        /// </summary>
        public bool CanSendForReview => Status == VacancyStatus.Submitted && IsDeleted == false;
        
    }
}
