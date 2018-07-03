﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks
{
    internal class VacancyOrchestratorTestData
    {
        internal static Vacancy GetPart1CompleteVacancy()
        {
            return new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value",
                Wage = new Wage { WageType = WageType.FixedWage }
            };
        }

        internal static VacancyUser GetVacancyUser()
        {
            return new VacancyUser
            {
                Email = "scotty@scotty.com",
                Name = "scott",
                UserId = "scott"
            };
        }
    }
}
