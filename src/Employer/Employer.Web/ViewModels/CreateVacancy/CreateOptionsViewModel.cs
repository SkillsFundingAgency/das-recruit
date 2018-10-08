using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy
{
    public class CreateOptionsViewModel 
    {
        public string VacancyId { get; set; }

        public List<ClonableVacancy> Vacancies { get; } = new List<ClonableVacancy>
        {
            new ClonableVacancy { Id = "VAC011010590", Name = "VAC011010590", Summary = "Apprenitces Business Analyst, closing 15 Oct 2018, status: pending review, IS Business Analyst, Level: Higher (Standard)" },
            new ClonableVacancy { Id = "VAC011010592", Name = "VAC011010592", Summary = "Apprentice Admin, closing 30 Spe 2018, status: live, Business Administrator, Level: Advanced (Standard)" },
            new ClonableVacancy { Id = "VAC011010593", Name = "VAC011010593", Summary = "Apprentice Legal Admin, closing 15 Sep 2018, status: closed, Business and Administration: Legal Administration, Level: Intermediate (Framework)" },
            new ClonableVacancy { Id = "VAC011010594", Name = "VAC011010594", Summary = "Apprentices Businesss Analst, closing 01 Sep 2018, status: closed, Business and Administration: Business Administration, Level: Intermediate (Framework)" }
        };
    }

    public class ClonableVacancy
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
    }
}
