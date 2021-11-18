using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace SFA.DAS.Recruit.Api.Models
{
    public class CreateVacancyRequest
    {
        public string Title { get ; set ; }
        public string Description { get ; set ; }
        public string ProgrammeId { get ; set ; }
        public string EmployerAccountId { get ; set ; }
        public VacancyUser User { get ; set ; }
        public string EmployerName { get ; set ; }
        public string ShortDescription { get ; set ; }
        public int NumberOfPositions { get ; set ; }
        public string OutcomeDescription { get ; set ; }
        
        public string AccountLegalEntityPublicHashedId { get ; set ; }
        public DateTime ClosingDate { get ; set ; }
        public DateTime StartDate { get ; set ; }
        
        public string LegalEntityName { get ; set ; }
        public string EmployerDescription { get ; set ; }
        public string TrainingDescription { get ; set ; }
        
        public CreateVacancyAddress Address { get; set; }
        public CreateVacancyWage Wage { get; set; }
        public List<string> Skills { get ; set ; }
        public EmployerNameOption EmployerNameOption { get ; set ; }
        public string AnonymousReason { get ; set ; }
        public List<CreateVacancyQualification> Qualifications { get; set; }
        public string ApplicationInstructions { get ; set ; }
        public string ApplicationUrl { get ; set ; }
        public CreateVacancyApplicationMethod ApplicationMethod { get ; set ; }
        public CreateVacancyDisabilityConfident DisabilityConfident { get ; set ; }
        public string ThingsToConsider { get ; set ; }
        public OwnerType OwnerType { get ; set ; }
        public string EmployerWebsiteUrl { get ; set ; }
        public ContactDetails EmployerContact { get; set; }
        public ContactDetails ProviderContact { get; set; }
    }

    public class ContactDetails
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
    
    public class CreateVacancyAddress
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }
    }

    public class CreateVacancyWage
    {
        public string WageAdditionalInformation { get ; set ; }
        public decimal? FixedWageYearlyAmount { get ; set ; }
        public decimal WeeklyHours { get ; set ; }
        public int Duration { get ; set ; }
        public string WorkingWeekDescription { get ; set ; }
        
        public WageType WageType { get; set; }
        public DurationUnit DurationUnit { get; set; }
    }

    public class CreateVacancyQualification
    {
        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public QualificationWeighting Weighting { get; set; }
    }
    
    public enum QualificationWeighting
    {
        Essential,
        Desired
    }
    public enum CreateVacancyApplicationMethod
    {
        ThroughFindAnApprenticeship,
        ThroughExternalApplicationSite
    }
    
    public enum WageType
    {
        FixedWage,
        NationalMinimumWageForApprentices,
        NationalMinimumWage,        
        Unspecified
    }
    
    public enum DurationUnit
    {
        Month,
        Year
    }
    
    public enum EmployerNameOption
    {
        RegisteredName,
        TradingName,
        Anonymous
    }
    public enum CreateVacancyDisabilityConfident
    {
        No = 0,
        Yes
    }
    public enum OwnerType
    {
        Employer,
        Provider,
        Joint
    }
}