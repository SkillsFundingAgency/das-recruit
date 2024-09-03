using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.Recruit.Api.Models;
using ApplicationMethod = Esfa.Recruit.Vacancies.Client.Domain.Entities.ApplicationMethod;
using DurationUnit = Esfa.Recruit.Vacancies.Client.Domain.Entities.DurationUnit;
using EmployerNameOption = Esfa.Recruit.Vacancies.Client.Domain.Entities.EmployerNameOption;
using QualificationWeighting = Esfa.Recruit.Vacancies.Client.Domain.Entities.QualificationWeighting;
using WageType = Esfa.Recruit.Vacancies.Client.Domain.Entities.WageType;

namespace SFA.DAS.Recruit.Api.Mappers
{
    public static class VacancyMapper
    {
        public static Vacancy MapFromCreateVacancyRequest(this CreateVacancyRequest request, Guid id)
        {
            return new Vacancy
            {
                EmployerLocation = new Address
                {
                    AddressLine1 = request.Address.AddressLine1,
                    AddressLine2 = request.Address.AddressLine2,
                    AddressLine3 = request.Address.AddressLine3,
                    AddressLine4 = request.Address.AddressLine4,
                    Postcode = request.Address.Postcode
                },
                Id = id,
                Description = request.Description,
                Title = request.Title,
                EmployerName = request.EmployerName,
                LegalEntityName = request.LegalEntityName,
                ApplicationMethod = (ApplicationMethod?)request.ApplicationMethod,
                ApplicationInstructions = request.ApplicationInstructions,
                ApplicationUrl = request.ApplicationUrl,
                OwnerType = (OwnerType)request.AccountType,
                Qualifications = request.Qualifications
                    .Select(c =>
                        new Qualification
                        {
                            Grade = c.Grade,
                            Subject = c.Subject,
                            Weighting = (QualificationWeighting?)c.Weighting,
                            QualificationType = c.QualificationType,
                            Level = c.Level,
                            OtherQualificationName = c.OtherQualificationName
                        })
                    .ToList(),
                Wage = new Wage
                {
                    WageType = (WageType)request.Wage.WageType,
                    WorkingWeekDescription = request.Wage.WorkingWeekDescription,
                    WeeklyHours = request.Wage.WeeklyHours,
                    Duration = request.Wage.Duration,
                    DurationUnit = Enum.Parse<DurationUnit>(request.Wage.DurationUnit.ToString(), true),
                    WageAdditionalInformation = request.Wage.WageAdditionalInformation,
                    FixedWageYearlyAmount = request.Wage.FixedWageYearlyAmount,
                    CompanyBenefitsInformation = request.Wage.CompanyBenefitsInformation
                },
                ShortDescription = request.ShortDescription,
                NumberOfPositions = request.NumberOfPositions,
                OutcomeDescription = request.OutcomeDescription,
                EmployerAccountId = request.EmployerAccountId,
                AccountLegalEntityPublicHashedId = request.AccountLegalEntityPublicHashedId,
                ClosingDate = request.ClosingDate,
                StartDate = request.StartDate,
                ProgrammeId = request.ProgrammeId,
                EmployerNameOption = (EmployerNameOption?)request.EmployerNameOption,
                AnonymousReason = request.AnonymousReason,
                EmployerDescription = request.EmployerDescription,
                TrainingDescription = request.TrainingDescription,
                AdditionalTrainingDescription = request.AdditionalTrainingDescription,
                Skills = request.Skills,
                DisabilityConfident = (DisabilityConfident)request.DisabilityConfident,
                ThingsToConsider = request.ThingsToConsider,
                CreatedByUser = request.User,
                AdditionalQuestion1 = request.AdditionalQuestion1,
                AdditionalQuestion2 = request.AdditionalQuestion2
            };
        }
    }
}