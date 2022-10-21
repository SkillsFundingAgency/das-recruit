using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Skills;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.Skills
{
    public class SkillsViewModel : ISkillsViewModel
    {
        public string Title { get; set; }
        public string AddCustomSkillName { get; set; }
        public List<string> Skills { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
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

        public bool IsTaskListCompleted { get; set; }
        
        public List<SkillViewModel> Column1Checkboxes { get; set; }
        public List<SkillViewModel> Column2Checkboxes { get; set; }
    }
}
