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
                AccountLegalEntityPublicHashedId = request.AccountLegalEntityPublicHashedId,
                AdditionalQuestion1 = request.AdditionalQuestion1,
                AdditionalQuestion2 = request.AdditionalQuestion2,
                AdditionalTrainingDescription = request.AdditionalTrainingDescription,
                AnonymousReason = request.AnonymousReason,
                ApplicationInstructions = request.ApplicationInstructions,
                ApplicationMethod = (ApplicationMethod?)request.ApplicationMethod,
                ApplicationUrl = request.ApplicationUrl,
                ClosingDate = request.ClosingDate,
                CreatedByUser = request.User,
                Description = request.Description,
                DisabilityConfident = (DisabilityConfident)request.DisabilityConfident,
                EmployerAccountId = request.EmployerAccountId,
                EmployerDescription = request.EmployerDescription,
                EmployerLocation = MapAddress(request.Address),
                EmployerLocationInformation = request.EmployerLocationInformation,
                EmployerLocationOption = request.EmployerLocationOption,
                EmployerLocations = request.Addresses?.Select(MapAddress).ToList(),
                EmployerName = request.EmployerName,
                EmployerNameOption = (EmployerNameOption?)request.EmployerNameOption,
                Id = id,
                LegalEntityName = request.LegalEntityName,
                NumberOfPositions = request.NumberOfPositions,
                OutcomeDescription = request.OutcomeDescription,
                OwnerType = (OwnerType)request.AccountType,
                ProgrammeId = request.ProgrammeId,
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
                ShortDescription = request.ShortDescription,
                Skills = request.Skills,
                StartDate = request.StartDate,
                ThingsToConsider = request.ThingsToConsider,
                Title = request.Title,
                TrainingDescription = request.TrainingDescription,
                Wage = new Wage
                {
                    WageType = Enum.Parse<WageType>(request.Wage.WageType.ToString()),
                    WorkingWeekDescription = request.Wage.WorkingWeekDescription,
                    WeeklyHours = request.Wage.WeeklyHours,
                    Duration = request.Wage.Duration,
                    DurationUnit = Enum.Parse<DurationUnit>(request.Wage.DurationUnit.ToString(), true),
                    WageAdditionalInformation = request.Wage.WageAdditionalInformation,
                    FixedWageYearlyAmount = request.Wage.FixedWageYearlyAmount,
                    CompanyBenefitsInformation = request.Wage.CompanyBenefitsInformation
                },
            };
        }

        private static Address MapAddress(CreateVacancyAddress source)
        {
            return source is null
                ? null
                : new Address
                    {
                        AddressLine1 = source.AddressLine1,
                        AddressLine2 = source.AddressLine2,
                        AddressLine3 = source.AddressLine3,
                        AddressLine4 = source.AddressLine4,
                        Postcode = source.Postcode
                    };
        }
    }
}