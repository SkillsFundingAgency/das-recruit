using System;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Title
{
    public class TitleEditModel
    {
        [FromRoute]
        public Guid? VacancyId { get; set; }
        public string Title { get; set; }
        public string EmployerAccountId { get; set; }
    }
}
