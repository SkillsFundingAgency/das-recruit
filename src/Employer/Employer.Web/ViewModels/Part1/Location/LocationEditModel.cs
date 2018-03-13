using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.Location
{
    public class LocationEditModel
    {
        [FromRoute]
        [Required, ValidGuid]
        public Guid VacancyId { get; set; }

        [Required]
        [FromRoute]
        public string EmployerAccountId { get; set; }

        public string SelectedOrganisationId { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }

        [Required, Postcode(ErrorMessage = "The Postcode is not valid.")]
        public string Postcode { get; set; }
    }
}
