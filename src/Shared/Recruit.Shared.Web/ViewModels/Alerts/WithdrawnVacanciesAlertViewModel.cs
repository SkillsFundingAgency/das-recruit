﻿using System.Collections.Generic;

namespace Esfa.Recruit.Shared.Web.ViewModels.Alerts
{
    public class WithdrawnVacanciesAlertViewModel
    {
        public List<string> ClosedVacancies { get; set; }
        
        public int ClosedVacanciesCount => ClosedVacancies.Count;

        public bool HasMultipleClosedVacancies => ClosedVacancies.Count > 1;
    }
}
