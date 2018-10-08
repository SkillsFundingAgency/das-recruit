﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy
{
    public class CreateOptionsViewModel 
    {
        public string VacancyId { get; set; }

        public IEnumerable<ClonableVacancy> Vacancies { get; set; }
    }

    public class ClonableVacancy
    {
        public Guid Id { get; set; }
        public long VacancyReference { get; set; }
        public string Summary { get; set; }
    }
}
