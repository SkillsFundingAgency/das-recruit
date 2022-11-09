using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer
{
    public class LegalEntityAndEmployerEditModel
    {
        [Required(ErrorMessage = ValidationMessages.EmployerSelectionValidationMessages.EmployerSelectionRequired)]
        public string SelectedOrganisationId { get; set; }

        public string SearchTerm { get; set; }
        public int Page { get; set; }
        public string EmployerAccountId { get; set; }
        public Guid? VacancyId { get; set; }
        [FromRoute]
        public long Ukprn { get; set; }
        public Dictionary<string, string> RouteDictionary
        {
            get
            {
                var routeDictionary = new Dictionary<string, string>
                {
                    {"Ukprn", Ukprn.ToString()}
                };  
                if(VacancyId != null)
                {
                    routeDictionary.Add("VacancyId", VacancyId.ToString());
                }
                return routeDictionary;
            }
        }
    }
}