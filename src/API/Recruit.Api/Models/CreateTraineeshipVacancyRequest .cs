using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace SFA.DAS.Recruit.Api.Models
{
    public class CreateTraineeshipVacancyRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int RouteId { get; set; }
        public string EmployerAccountId { get; set; }
        public VacancyUser User { get; set; }
        public string EmployerName { get; set; }
        public string ShortDescription { get; set; }
        public int NumberOfPositions { get; set; }
        public string OutcomeDescription { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public DateTime ClosingDate { get; set; }
        public DateTime StartDate { get; set; }
        public string LegalEntityName { get; set; }
        public string EmployerDescription { get; set; }
        public string TrainingDescription { get; set; }
        public CreateTraineeshipVacancyAddress Address { get; set; }
        public CreateTraineeshipVacancyWage Wage { get; set; }
        public List<string> Skills { get; set; }
        public TraineeshipEmployerNameOption EmployerNameOption { get; set; }
        public string AnonymousReason { get; set; }
        public CreateTraineeshipVacancyDisabilityConfident DisabilityConfident { get; set; }
        public string WorkExperience { get; set; }
    }

    public class CreateTraineeshipVacancyAddress
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }
    }

    public class CreateTraineeshipVacancyWage
    {
        public decimal WeeklyHours { get; set; }
        public int Duration { get; set; }
        public string WorkingWeekDescription { get; set; }
        public TraineeshipDurationUnit DurationUnit { get; set; }
    }
    public enum TraineeshipDurationUnit
    {
        Week,
        Month
    }
    public enum TraineeshipEmployerNameOption
    {
        RegisteredName,
        TradingName,
        Anonymous
    }
    public enum CreateTraineeshipVacancyDisabilityConfident
    {
        No = 0,
        Yes
    }
}