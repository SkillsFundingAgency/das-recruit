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
        public OwnerType OwnerType { get; set; }
        public SourceOrigin SourceOrigin { get; set; }
        public SourceType SourceType { get; set; }
        public long? SourceVacancyReference { get; set; }
        public DateTime? ClosedDate { get; set; }
        public VacancyUser ClosedByUser { get; set; }
        public DateTime? CreatedDate { get; set; }
        public VacancyUser CreatedByUser { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public VacancyUser SubmittedByUser { get; set; }
        public DateTime? ReviewDate { get; set; }
        public VacancyUser ReviewByUser { get; set; }
        public int ReviewCount { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? LiveDate { get; set; }

        public DateTime? LastUpdatedDate { get; set; }
        public VacancyUser LastUpdatedByUser { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public VacancyUser DeletedByUser { get; set; }
        public string AnonymousReason { get; set; }
        public string ApplicationInstructions { get; set; }
        public ApplicationMethod? ApplicationMethod { get; set; }
        public string ApplicationUrl { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string Description { get; set; }
        public DisabilityConfident DisabilityConfident { get; set; }
        public ContactDetail EmployerContact { get; set; }
        public string EmployerDescription { get; set; }
        public Address EmployerLocation { get; set; }
        public string EmployerName { get; set; }
        public EmployerNameOption? EmployerNameOption { get; set; }
        public List<EmployerReviewFieldIndicator> EmployerReviewFieldIndicators { get; set; }
        public string EmployerRejectedReason { get; set; }
        public string LegalEntityName { get; set; }
        public string EmployerWebsiteUrl { get; set; }
        public GeoCodeMethod? GeoCodeMethod { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public int? NumberOfPositions { get; set; }
        public string OutcomeDescription { get; set; }
        public ContactDetail ProviderContact { get; set; }
        public List<ProviderReviewFieldIndicator> ProviderReviewFieldIndicators { get; set; }
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
        public ClosureReason? ClosureReason { get; set; }
        public string ClosureExplanation { get; set; }
        public TransferInfo TransferInfo { get; set; }
        public bool CanClose => Status == VacancyStatus.Live;
        public bool CanClone => (Status == VacancyStatus.Live || 
                                 Status == VacancyStatus.Closed || 
                                 Status == VacancyStatus.Submitted || 
                                 Status == VacancyStatus.Review);
        /// <summary>
        /// We can only delete draft vacancies that have not been deleted
        /// </summary>
        public bool CanDelete => (Status == VacancyStatus.Draft ||
                                  Status == VacancyStatus.Referred ||
                                  Status == VacancyStatus.Rejected)
                                 && IsDeleted == false;
        /// <summary>
        /// We can only edit draft & referred & rejected vacancies that have not been deleted
        /// </summary>
        public bool CanEdit => (Status == VacancyStatus.Draft || 
                                Status == VacancyStatus.Referred ||
                                Status == VacancyStatus.Rejected)                               
                                && IsDeleted == false;

        /// <summary>
        /// Employer can only edit draft , referred & review vacancies that have not been deleted
        /// </summary>
        public bool CanEmployerEdit => (Status == VacancyStatus.Draft ||
                                Status == VacancyStatus.Referred ||
                                Status == VacancyStatus.Review)                                
                                && IsDeleted == false;

        public bool CanGetEmployerProfileAboutOrganisation => (Status == VacancyStatus.Draft ||
                                Status == VacancyStatus.Referred)                                
                                && IsDeleted == false;

        /// <summary>
        /// The vacancy is being edited
        /// We can only submit draft & referred & rejected vacancies that have not been deleted
        /// </summary>
        public bool CanSubmit => (Status == VacancyStatus.Draft || Status == VacancyStatus.Referred || Status == VacancyStatus.Rejected || Status == VacancyStatus.Review) && IsDeleted == false;

        /// <summary>
        /// We can only approve submitted vacancies that have not been deleted
        /// </summary>
        public bool CanApprove => (Status == VacancyStatus.Submitted) && IsDeleted == false;

        /// <summary>
        /// We can only refer pending review vacancies that have not been deleted
        /// </summary>
        public bool CanRefer => Status == VacancyStatus.Submitted && IsDeleted == false;

        /// <summary>
        /// We can only reject  review vacancies that have not been deleted
        /// </summary>
        public bool CanReject => Status == VacancyStatus.Review && IsDeleted == false;

        /// <summary>
        /// We can only make approved vacancies live that have not been deleted
        /// </summary>
        public bool CanMakeLive => Status == VacancyStatus.Approved && IsDeleted == false;

        /// <summary>
        /// We can send for review vacancies that are submitted and that have not been deleted
        /// </summary>
        public bool CanSendForReview => Status == VacancyStatus.Submitted && IsDeleted == false;
        
        public bool CanReview => Status == VacancyStatus.Review && IsDeleted == false;

        public bool CanEmployerAndProviderCollabarate => (Status == VacancyStatus.Review || Status == VacancyStatus.Rejected);

        public bool IsDisabilityConfident => DisabilityConfident == DisabilityConfident.Yes;

        /// <summary>
        /// We can extend the ClosingDate and StartDate for Live vacancies that have not been deleted
        /// </summary>
        public bool CanExtendStartAndClosingDates => Status == VacancyStatus.Live && IsDeleted == false;

        /// <summary>
        /// Is the employer anonymous
        /// </summary>
        public bool IsAnonymous => EmployerNameOption == Entities.EmployerNameOption.Anonymous;

        /// <summary>
        /// Should the vacancy be geocoded using the outcode part of the postcode
        /// </summary>
        public bool GeocodeUsingOutcode => IsAnonymous;
        
        /// <summary>
        /// Type of Vacancy being created, either Apprenticeship or Traineeship. Set by application startup.
        /// </summary>
        public VacancyType? VacancyType { get; set; }
    }
}
