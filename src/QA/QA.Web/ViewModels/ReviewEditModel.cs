using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class ReviewEditModel
    {
        [FromRoute]
        public Guid ReviewId {get;set;}

        public List<string> SelectedFieldIdentifiers { get; set; } = new List<string>();
        public List<string> SelectedAutomatedQaResults { get; set; } = new List<string>();

        [Required(ErrorMessage = "Please add a comment")]
        public string ReviewerComment { get; set; }

        public bool IsRefer => SelectedFieldIdentifiers.Any() || SelectedAutomatedQaResults.Any();
        
        public string ShortDescription { get; set; }
        public string WorkingWeekDescription { get; set; }
        public string CompanyBenefitsInformation { get; set; }
        public string VacancyDescription { get; set; }
        public string TrainingDescription { get; set; }
        public string AdditionalTrainingDescription { get; set; }
        public string OutcomeDescription { get; set; }
        public string EmployerLocationInformation { get; set; }
    }
}
