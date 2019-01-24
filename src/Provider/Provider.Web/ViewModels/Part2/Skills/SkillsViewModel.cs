﻿using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.Skills
{
    public class SkillsViewModel : SkillsEditModel
    {
        public string Title { get; set; }
        public List<SkillViewModel> Column1Checkboxes { get; set; }
        public List<SkillViewModel> Column2Checkboxes { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
    }

    public class SkillViewModel
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
        public string Value { get; set; }
    }
}
