﻿using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.Recruit.Api.Models;
using ApplicationMethod = Esfa.Recruit.Vacancies.Client.Domain.Entities.ApplicationMethod;
using DurationUnit = Esfa.Recruit.Vacancies.Client.Domain.Entities.DurationUnit;
using EmployerNameOption = Esfa.Recruit.Vacancies.Client.Domain.Entities.EmployerNameOption;

namespace SFA.DAS.Recruit.Api.Mappers
{
    public static class TraineeshipVacancyMapper
    {
        public static Vacancy MapFromCreateTraineeshipVacancyRequest(this CreateTraineeshipVacancyRequest request, Guid id)
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
                Wage = new Wage
                {
                    WorkingWeekDescription = request.Wage.WorkingWeekDescription,
                    WeeklyHours = request.Wage.WeeklyHours,
                    Duration = request.Wage.Duration,
                    DurationUnit = (DurationUnit?)request.Wage.DurationUnit,
                },
                ShortDescription = request.ShortDescription,
                NumberOfPositions = request.NumberOfPositions,
                OutcomeDescription = request.OutcomeDescription,
                EmployerAccountId = request.EmployerAccountId,
                AccountLegalEntityPublicHashedId = request.AccountLegalEntityPublicHashedId,
                ClosingDate = request.ClosingDate,
                StartDate = request.StartDate,
                RouteId = request.RouteId,
                EmployerNameOption = (EmployerNameOption?)request.EmployerNameOption,
                AnonymousReason = request.AnonymousReason,
                EmployerDescription = request.EmployerDescription,
                TrainingDescription = request.TrainingDescription,
                Skills = request.Skills,
                DisabilityConfident = (DisabilityConfident)request.DisabilityConfident,
                WorkExperience = request.WorkExperience,
                ApplicationMethod = ApplicationMethod.ThroughFindATraineeship,
                CreatedByUser = request.User
            };
        }
    }
}