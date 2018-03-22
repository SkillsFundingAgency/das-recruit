using System;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Microsoft.AspNetCore.Mvc;
using ErrMsg = Esfa.Recruit.Employer.Web.ViewModels.ValidationMessages.TitleValidationMessages;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Title
{
    public class TitleEditModel
    {
        [FromRoute]
        [Required]
        public string EmployerAccountId { get; set; }

        [FromRoute]
        [ValidGuid]
        public Guid? VacancyId { get; set; }

        public string Title { get; set; }
    }
}
