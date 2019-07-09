﻿using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Training
{
    public class ConfirmTrainingViewModel
    {
        public string TrainingTitle { get; set; }
        public ProgrammeLevel Level { get; set; }
        public int DurationMonths { get; set; }
        public string ProgrammeType {get; set; }
        public string ProgrammeId { get; set; }
        public PartOnePageInfoViewModel PageInfo { get; set; }
    }
}
