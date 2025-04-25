using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Training
{
    public class ConfirmTrainingViewModel : VacancyRouteModel
    {
        public string Title { get; set; }
        public string TrainingTitle { get; set; }
        public ApprenticeshipLevel ApprenticeshipLevel { get; set; }
        public string EducationLevelName { get; set; }
        public int DurationMonths { get; set; }
        public string ProgrammeType {get; set; }
        public string ProgrammeId { get; set; }
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public string TrainingEffectiveToDate { get; set; }
        
        public bool ShowTrainingEffectiveToDate => string.IsNullOrWhiteSpace(TrainingEffectiveToDate) == false;
        public bool IsFoundation { get; set; }
    }
}
