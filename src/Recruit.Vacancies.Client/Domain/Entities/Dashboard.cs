﻿using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Dashboard
    {
        public Guid Id { get; set; }

        public string EmployerAccountId { get; set; }

        public IEnumerable<VacancySummary> Vacancies { get; set; }
    }
}
