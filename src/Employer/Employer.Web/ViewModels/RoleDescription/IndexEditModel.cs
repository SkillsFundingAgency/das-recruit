﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.RoleDescription
{
    public class IndexEditModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        [FromRoute]
        public Guid VacancyId { get; set; }
    }
}
