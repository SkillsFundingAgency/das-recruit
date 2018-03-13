using System;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer
{
    public class EmployerEditModel
    {
        [FromRoute]
        [Required, ValidGuid]
        public Guid VacancyId { get; set; }

        [Required]
        [FromRoute]
        public string EmployerAccountId { get; set; }

        [Required(ErrorMessage = "You must select a legal entity.")]
        public string SelectedOrganisationId { get; set; }

        [Display(Name = "Address Line 1")]
        [Required, FreeText, StringLength(100)]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        [FreeText, StringLength(100)]
        public string AddressLine2 { get; set; }

        [Display(Name = "Address Line 3")]
        [FreeText, StringLength(100)]
        public string AddressLine3 { get; set; }

        [Display(Name = "Address Line 4")]
        [FreeText, StringLength(100)]
        public string AddressLine4 { get; set; }

        [Required, Postcode]
        public string Postcode { get; set; }
    }
}
