using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy
{
    public class CreateOptionsViewModel 
    {
        public string VacancyId { get; set; }

        public List<ClonableVacancy> Vacancies { get; } = new List<ClonableVacancy>
        {
            new ClonableVacancy { Id = "VAC111", Name = "VAC111" },
            new ClonableVacancy { Id = "VAC222", Name = "VAC222" },
            new ClonableVacancy { Id = "VAC333", Name = "VAC333" }
        };
    }

    public class ClonableVacancy
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
