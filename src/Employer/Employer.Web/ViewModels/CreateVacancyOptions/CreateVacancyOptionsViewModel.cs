using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.CreateVacancyOptions
{
    public class CreateVacancyOptionsViewModel : CreateVacancyOptionsEditModel
    {
        public IEnumerable<ClonableVacancy> Vacancies { get; set; }
    }

    public class ClonableVacancy
    {
        public Guid Id { get; set; }
        public long VacancyReference { get; set; }
        public string Summary { get; set; }
    }
}
