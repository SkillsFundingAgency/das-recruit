using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Vacancy
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
        
        public string EmployerAccountId { get; internal set; }

        public string ApplicationInstructions { get; set; }
        public string ApplicationUrl { get; set; }
        
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; internal set; }
        public string Description { get; set; }
        public string EmployerContactName { get; set; }
        public string EmployerContactEmail { get; set; }
        public string EmployerContactPhone { get; set; }
        public string EmployerDescription { get; set; }
        public string EmployerWebsiteUrl { get; set; }
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

        public string OrganisationName { get; set; }
        public string OutcomeDescription { get; set; }
        public Address Location { get; set; }

        public int? NumberOfPositions { get; set; }

        public string ShortDescription { get; set; }
        public string ThingsToConsider { get; set; }
        public DateTime? ClosingDate { get; set; }

        public DateTime? StartDate { get; set; }

        public Programme Programme { get; set; }

        public Wage Wage { get; set; }

        public IEnumerable<string> Skills { get; set; }
        public string TrainingDescription { get; set; }
        
    }
}
