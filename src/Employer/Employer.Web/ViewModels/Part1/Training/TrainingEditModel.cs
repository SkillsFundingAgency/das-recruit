using System;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    using System.ComponentModel.DataAnnotations;
    using Esfa.Recruit.Employer.Web.ViewModels.Validations;
    using ErrMsg = Esfa.Recruit.Employer.Web.ViewModels.ValidationMessages.TrainingValidationMessages;

    public class TrainingEditModel
    {
        [FromRoute]
        [Required]
        public string EmployerAccountId { get; set; }

        [FromRoute]
        [ValidGuid]
        public Guid VacancyId { get; set; }

        public string ClosingDay { get; set; }
        public string ClosingMonth { get; set; }
        public string ClosingYear { get; set; }

        [Required(ErrorMessage = ErrMsg.Required.ClosingDate)]
        [TypeOfDate(ErrorMessage = ErrMsg.TypeOfDate.ClosingDate)]
        public string ClosingDate => $"{ClosingDay}/{ClosingMonth}/{ClosingYear}";

        public string StartDay { get; set; }
        public string StartMonth { get; set; }
        public string StartYear { get; set; }

        [Required(ErrorMessage = ErrMsg.Required.StartDate)]
        [TypeOfDate(ErrorMessage = ErrMsg.TypeOfDate.StartDate)]
        public string StartDate => $"{StartDay}/{StartMonth}/{StartYear}";

    }
}
