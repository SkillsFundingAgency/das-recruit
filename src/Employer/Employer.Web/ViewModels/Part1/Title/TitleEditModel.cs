﻿using System;
using Microsoft.AspNetCore.Mvc;


namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Title
{
    public class TitleEditModel
    {
        [FromRoute]
        public string EmployerAccountId { get; set; }

        [FromRoute]
        public Guid? VacancyId { get; set; }

        public string Title { get; set; }
    }
}
