namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Esfa.Recruit.Employer.Web.ViewModels.Validations;
    using Microsoft.AspNetCore.Mvc;
    using ErrMsg = ValidationMessages.ShortDescriptionValidationMessages;

    public class ShortDescriptionEditModel
    {
        [Required]
        [FromRoute]
        public string EmployerAccountId { get; set; }

        [FromRoute]
        [ValidGuid]
        public Guid VacancyId { get; set; }

        [TypeOfInteger(ErrorMessage = ErrMsg.TypeOfInteger.NumberOfPositions)]
        public string NumberOfPositions { get; set; }

        public string ShortDescription { get; set; }
    }
}
