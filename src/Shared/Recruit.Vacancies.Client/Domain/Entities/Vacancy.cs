using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Vacancy
    {
        public Guid Id { get; set; }
        public string EmployerAccountId { get; set; }
        public long? VacancyReference { get; set; }
        public VacancyStatus Status { get; set; }
        public SourceOrigin SourceOrigin { get; set; }
        public SourceType SourceType { get; set; }
        public long? SourceVacancyReference { get; set; }

        public DateTime? ClosedDate { get; set; }
        public VacancyUser ClosedByUser { get; set; }

        public DateTime? CreatedDate { get; set; }
        public VacancyUser CreatedByUser { get; set; }

        public DateTime? SubmittedDate { get; set; }
        public VacancyUser SubmittedByUser { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime? LiveDate { get; set; }

        public DateTime? LastUpdatedDate { get; set; }
        public VacancyUser LastUpdatedByUser { get; set; }
        
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public VacancyUser DeletedByUser { get; set; }
        
        public string ApplicationInstructions { get; set; }
        public ApplicationMethod? ApplicationMethod { get; set; }
        public string ApplicationUrl { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string Description { get; set; }
        public DisabilityConfident DisabilityConfident { get; set; }
        public string EmployerContactEmail { get; set; }
        public string EmployerContactName { get; set; }
        public string EmployerContactPhone { get; set; }
        public string EmployerDescription { get; set; }
        public Address EmployerLocation { get; set; }
        public string EmployerName { get; set; }
        public GeoCodeMethod? GeoCodeMethod { get; set; }
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

        public bool CanClose => Status == VacancyStatus.Live;

        /// <summary>
        /// We can only delete draft vacancies that have not been deleted
        /// </summary>
        public bool CanDelete => Status == VacancyStatus.Draft && IsDeleted == false;

        /// <summary>
        /// We can only edit draft & referred vacancies that have not been deleted
        /// </summary>
        public bool CanEdit => (Status == VacancyStatus.Draft || 
                                Status == VacancyStatus.Referred ) 
                               && IsDeleted == false;

        /// <summary>
        /// We can only submit draft & referred vacancies that have not been deleted
        /// </summary>
        public bool CanSubmit => (Status == VacancyStatus.Draft || Status == VacancyStatus.Referred) && IsDeleted == false;

        /// <summary>
        /// We can only approve submitted vacancies that have not been deleted
        /// </summary>
        public bool CanApprove => (Status == VacancyStatus.Submitted) && IsDeleted == false;

        /// <summary>
        /// We can only refer pending review vacancies that have not been deleted
        /// </summary>
        public bool CanRefer => Status == VacancyStatus.Submitted && IsDeleted == false;

        /// <summary>
        /// We can only make approved vacancies live that have not been deleted
        /// </summary>
        public bool CanMakeLive => Status == VacancyStatus.Approved && IsDeleted == false;

        /// <summary>
        /// We can send for review vacancies that are submitted and that have not been deleted
        /// </summary>
        public bool CanSendForReview => Status == VacancyStatus.Submitted && IsDeleted == false;

        public bool IsDisabilityConfident => DisabilityConfident == DisabilityConfident.Yes;
    }
}
