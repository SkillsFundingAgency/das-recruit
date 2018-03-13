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

        [Required(ErrorMessage = ErrMsg.Required.NumberOfPositions)]
        [Range(1, Double.MaxValue, ErrorMessage = ErrMsg.Range.NumberOfPositions)]
        public int? NumberOfPositions { get; set; }

        [Required(ErrorMessage = ErrMsg.Required.ShortDescription)]
        [StringLength(maximumLength:350, MinimumLength = 50, ErrorMessage = ErrMsg.StringLength.ShortDescription)]
        [FreeText(ErrorMessage = ErrMsg.FreeText.ShortDescription)]
        public string ShortDescription { get; set; }
    }
}
