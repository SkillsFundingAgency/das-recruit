using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Models;
using LiveVacancy = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy.LiveVacancy;
using ProjectionAddress = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy.Address;
using ProjectionQualification = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy.Qualification;
using ProjectionTrainingProvider = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy.TrainingProvider;
using ProjectionWage = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy.Wage;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions
{
    public static class VacancyExtensions
    {
        public static LiveVacancy ToLiveVacancyProjection(this Vacancy vacancy, ApprenticeshipProgramme programme)
        {
            return new LiveVacancy
            {
                VacancyId = vacancy.Id,
                ApplicationInstructions = vacancy.ApplicationInstructions,
                ApplicationMethod = vacancy.ApplicationMethod.Value.ToString(),
                ApplicationUrl = vacancy.ApplicationUrl,
                ClosingDate = vacancy.ClosingDate.Value,
                Description = vacancy.Description,
                EmployerContactEmail = vacancy.EmployerContactEmail,
                EmployerContactName = vacancy.EmployerContactName,
                EmployerContactPhone = vacancy.EmployerContactPhone,
                EmployerDescription = vacancy.EmployerDescription,
                EmployerLocation = vacancy.EmployerLocation.ToProjection(),
                EmployerName = vacancy.EmployerName,
                EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
                LiveDate = vacancy.LiveDate.Value,
                NumberOfPositions = vacancy.NumberOfPositions.Value,
                OutcomeDescription = vacancy.OutcomeDescription,
                ProgrammeId = vacancy.ProgrammeId,
                ProgrammeLevel = programme.Level.ToString(),
                ProgrammeType = programme.ApprenticeshipType.ToString(),
                Qualifications = vacancy.Qualifications.ToProjection(),
                ShortDescription = vacancy.ShortDescription,
                Skills = vacancy.Skills,
                StartDate = vacancy.StartDate.Value,
                ThingsToConsider = vacancy.ThingsToConsider,
                Title = vacancy.Title,
                TrainingDescription = vacancy.TrainingDescription,
                TrainingProvider = vacancy.TrainingProvider.ToProjection(),
                VacancyReference = vacancy.VacancyReference.Value,
                Wage = vacancy.Wage.ToProjection()
            };
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
                Latitude = address.Latitude.Value, 
                Longitude = address.Longitude.Value
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
