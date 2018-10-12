using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;
using ProjectionAddress = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy.Address;
using ProjectionQualification = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy.Qualification;
using ProjectionTrainingProvider = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy.TrainingProvider;
using ProjectionWage = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy.Wage;
using Qualification = Esfa.Recruit.Vacancies.Client.Domain.Entities.Qualification;
using TrainingProvider = Esfa.Recruit.Vacancies.Client.Domain.Entities.TrainingProvider;
using Wage = Esfa.Recruit.Vacancies.Client.Domain.Entities.Wage;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions
{
    public static class VacancyExtensions
    {
        public static T ToVacancyProjectionBase<T>(this Vacancy vacancy, ApprenticeshipProgramme programme, Func<string> getDocumentId) where T : VacancyProjectionBase
        {
            var projectedVacancy = (T) Activator.CreateInstance<T>();

            projectedVacancy.Id = getDocumentId();
            projectedVacancy.LastUpdated = DateTime.UtcNow;
            projectedVacancy.VacancyId = vacancy.Id;
            projectedVacancy.ApplicationInstructions = vacancy.ApplicationInstructions;
            projectedVacancy.ApplicationMethod = vacancy.ApplicationMethod.GetValueOrDefault().ToString();
            projectedVacancy.ApplicationUrl = vacancy.ApplicationUrl;
            projectedVacancy.ClosingDate = vacancy.ClosingDate.GetValueOrDefault();
            projectedVacancy.Description = vacancy.Description;
            projectedVacancy.DisabilityConfident = vacancy.DisabilityConfident;
            projectedVacancy.EmployerContactEmail = vacancy.EmployerContactEmail;
            projectedVacancy.EmployerContactName = vacancy.EmployerContactName;
            projectedVacancy.EmployerContactPhone = vacancy.EmployerContactPhone;
            projectedVacancy.EmployerDescription = vacancy.EmployerDescription;
            projectedVacancy.EmployerLocation = vacancy.EmployerLocation.ToProjection();
            projectedVacancy.EmployerName = vacancy.EmployerName;
            projectedVacancy.EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl;
            projectedVacancy.LiveDate = vacancy.LiveDate.GetValueOrDefault();
            projectedVacancy.NumberOfPositions = vacancy.NumberOfPositions.GetValueOrDefault();
            projectedVacancy.OutcomeDescription = vacancy.OutcomeDescription;
            projectedVacancy.ProgrammeId = vacancy.ProgrammeId;
            projectedVacancy.ProgrammeLevel = programme.Level.ToString();
            projectedVacancy.ProgrammeType = programme.ApprenticeshipType.ToString();
            projectedVacancy.Qualifications = vacancy.Qualifications.ToProjection();
            projectedVacancy.ShortDescription = vacancy.ShortDescription;
            projectedVacancy.Skills = vacancy.Skills;
            projectedVacancy.StartDate = vacancy.StartDate.GetValueOrDefault();
            projectedVacancy.ThingsToConsider = vacancy.ThingsToConsider;
            projectedVacancy.Title = vacancy.Title;
            projectedVacancy.TrainingDescription = vacancy.TrainingDescription;
            projectedVacancy.TrainingProvider = vacancy.TrainingProvider.ToProjection();
            projectedVacancy.VacancyReference = vacancy.VacancyReference.GetValueOrDefault();
            projectedVacancy.Wage = vacancy.Wage.ToProjection();

            return projectedVacancy;
        }

        public static ProjectionAddress ToProjection(this Address address)
        {
            return new ProjectionAddress
            {
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3,
                AddressLine4 = address.AddressLine4,
                Postcode = address.Postcode,
                Latitude = address.Latitude.GetValueOrDefault(), 
                Longitude = address.Longitude.GetValueOrDefault()
            };
        }

        public static IEnumerable<ProjectionQualification> ToProjection(this List<Qualification> qualifications)
        {
            return qualifications.Select(q => new ProjectionQualification
            {
                QualificationType = q.QualificationType,
                Subject = q.Subject,
                Grade = q.Grade,
                Weighting = q.Weighting.Value.ToString()
            });
        }


        public static ProjectionTrainingProvider ToProjection(this TrainingProvider trainingProvider)
        {
            return new ProjectionTrainingProvider
            {
                Name = trainingProvider.Name,
                Ukprn = trainingProvider.Ukprn.Value
            };
        }

        public static ProjectionWage ToProjection(this Wage wage)
        {
            return new ProjectionWage
            {
                Duration = wage.Duration.Value,
                DurationUnit = wage.DurationUnit.Value.ToString(),
                FixedWageYearlyAmount = wage.FixedWageYearlyAmount,
                WageAdditionalInformation = wage.WageAdditionalInformation,
                WageType = wage.WageType.Value.ToString(),
                WeeklyHours = wage.WeeklyHours.Value,
                WorkingWeekDescription = wage.WorkingWeekDescription
            };
        }
    }
}
