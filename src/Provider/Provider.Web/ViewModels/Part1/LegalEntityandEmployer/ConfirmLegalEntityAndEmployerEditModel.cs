using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer
{
    public class ConfirmLegalEntityAndEmployerEditModel
    {
        [Required(ErrorMessage = "You must confirm the employer")]
        public bool? HasConfirmedEmployer { get; set; }
        public string EmployerAccountId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string EmployerName { get; set; }
        [FromRoute]
        public long Ukprn { get; set; }

        public Guid? VacancyId { get; set; }

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