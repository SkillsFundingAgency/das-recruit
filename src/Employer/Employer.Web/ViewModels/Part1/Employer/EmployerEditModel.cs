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
        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        [Required(ErrorMessage = ErrMsg.Required.Postcode)]
        public string Postcode { get; set; }
    }
}
