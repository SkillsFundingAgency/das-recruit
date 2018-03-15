namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Validations;
    using Microsoft.AspNetCore.Mvc;
    using ErrMsg = ValidationMessages.EmployerEditModelValidationMessages;

    public class EmployerEditModel
    {
        [FromRoute]
        [Required, ValidGuid]
        public Guid VacancyId { get; set; }

        [Required]
        [FromRoute]
        public string EmployerAccountId { get; set; }

        [Required(ErrorMessage = ErrMsg.Required.SelectedOrganisationId)]
        public string SelectedOrganisationId { get; set; }

        [Required(ErrorMessage = ErrMsg.Required.AddressLine1)]
        [FreeText(ErrorMessage = ErrMsg.FreeText.AddressLine1)]
        [StringLength(100, ErrorMessage = ErrMsg.StringLength.AddressLine1)]
        public string AddressLine1 { get; set; }

        [FreeText(ErrorMessage = ErrMsg.FreeText.AddressLine2)]
        [StringLength(100, ErrorMessage = ErrMsg.StringLength.AddressLine2)]
        public string AddressLine2 { get; set; }

        [FreeText(ErrorMessage = ErrMsg.FreeText.AddressLine3)]
        [StringLength(100, ErrorMessage = ErrMsg.StringLength.AddressLine3)]
        public string AddressLine3 { get; set; }

        [FreeText(ErrorMessage = ErrMsg.FreeText.AddressLine4)]
        [StringLength(100, ErrorMessage = ErrMsg.StringLength.AddressLine4)]
        public string AddressLine4 { get; set; }

        [Required(ErrorMessage = ErrMsg.Required.Postcode)]
        [Postcode(ErrorMessage = ErrMsg.PostcodeAttribute.Postcode)]
        public string Postcode { get; set; }
    }
}
