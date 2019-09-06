using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    public class ConfirmTrainingViewModel
    {
        public string TrainingTitle { get; set; }
        public ApprenticeshipLevel Level { get; set; }
        public int DurationMonths { get; set; }
        public string ProgrammeType {get; set; }
        public string ProgrammeId { get; set; }
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public string TrainingEffectiveToDate { get; set; }
        public bool ShowTrainingEffectiveToDate => string.IsNullOrWhiteSpace(TrainingEffectiveToDate) == false;
        public string EducationLevelName { get; set; }
    }
}
