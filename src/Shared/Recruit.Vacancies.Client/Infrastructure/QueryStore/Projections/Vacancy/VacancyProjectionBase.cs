﻿using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy
{
    public abstract class VacancyProjectionBase : QueryProjectionBase
    {
        protected VacancyProjectionBase(string viewType) : base(viewType)
        {}

        public Guid VacancyId { get; set; }
        public string ApplicationInstructions { get; set; }
        public string ApplicationMethod { get; set; }
        public string ApplicationUrl { get; set; }
        public DateTime ClosingDate { get; set; }
        public string Description { get; set; }
        public DisabilityConfident DisabilityConfident { get; set; }
        public string EmployerContactEmail { get; set; }
        public string EmployerContactName { get; set; }
        public string EmployerContactPhone { get; set; }
        public string EmployerDescription { get; set; }
        public Address EmployerLocation { get; set; }
        public List<Address> EmployerLocations { get; set; }
        public AvailableWhere? EmployerLocationOption { get; set; }
        public string EmployerLocationInformation { get; set; }
        public string EmployerName { get; set; }
        public string EmployerWebsiteUrl { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime LiveDate { get; set; }
        public int NumberOfPositions { get; set; }
        public string OutcomeDescription { get; set; }
        public string ProgrammeId { get; set; }
        public string ProgrammeLevel { get; set; }
        public string ProgrammeType { get; set; }
        public string ProviderContactEmail { get; set; }
        public string ProviderContactName { get; set; }
        public string ProviderContactPhone { get; set; }
        public IEnumerable<Qualification> Qualifications { get; set; }
        public string ShortDescription { get; set; }
        public IEnumerable<string> Skills { get; set; }
        public DateTime StartDate { get; set; }
        public string ThingsToConsider { get; set; }
        public string Title { get; set; }
        public string TrainingDescription { get; set; }
        public TrainingProvider TrainingProvider { get; set; }
        public long VacancyReference { get; set; }
        public Wage Wage { get; set; }
        public int? EducationLevelNumber { get; set; }
        public string AccountPublicHashedId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string AdditionalQuestion1 { get; set; }
        public string AdditionalQuestion2 { get; set; }
        public string AdditionalTrainingDescription { get; set; }
        public ApprenticeshipTypes ApprenticeshipType { get; set; }
    }
}
