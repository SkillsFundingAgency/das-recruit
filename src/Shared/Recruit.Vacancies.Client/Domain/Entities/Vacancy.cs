using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using System;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Vacancy
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
        
        public string EmployerAccountId { get; internal set; }

        public DateTime? CreatedDate { get; set; }

        public VacancyStatus Status { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }

        /// <summary>
        /// We can only submit draft vacancies that have not been deleted
        /// </summary>
        public bool CanSubmit => Status == VacancyStatus.Draft && IsDeleted == false;

        /// <summary>
        /// We can only delete draft vacancies that have not been deleted
        /// </summary>
        public bool CanDelete => Status == VacancyStatus.Draft && IsDeleted == false;

        /// <summary>
        /// We can only edit draft vacancies that have not been deleted
        /// </summary>
        public bool CanEdit => Status == VacancyStatus.Draft && IsDeleted == false;

        public long? Ukprn { get; set; }
        public string ProviderName { get; set; }
        public string ProviderAddress { get; set; }

        public string OrganisationId { get; set; }
        
        public Address Location { get; set; }

        public int? NumberOfPositions { get; set; }

        public string ShortDescription { get; set; }

        public DateTime? ClosingDate { get; set; }

        public DateTime? StartDate { get; set; }

        public string ProgrammeId { get; set; }

        public string ProgrammeTitle { get; set; }

        public TrainingType? TrainingType { get; set; }

        public Wage Wage { get; set; }
    }
}
