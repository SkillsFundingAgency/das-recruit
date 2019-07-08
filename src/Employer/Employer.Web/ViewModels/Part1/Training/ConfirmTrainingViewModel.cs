using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    public class ConfirmTrainingViewModel
    {
        public string TrainingTitle { get; set; }
        public ProgrammeLevel Level { get; set; }
        public int DurationMonths { get; set; }
        public string ProgrammeType {get; set; }
        public string ProgrammeId { get; set; }
        public PartOnePageInfoViewModel PageInfo { get; set; }

        public string LevelAsText
        {
            get
            {
                switch (Level)
                {
                    case ProgrammeLevel.Intermediate:
                        return "Level:2 (equivalent to GCSEs at grades A* to C)";
                    case ProgrammeLevel.Advanced:
                        return "Level:3 (equivalent to A levels at grades A to E)";
                    case ProgrammeLevel.Higher:
                        return "Level:4 (equivalent to certificate of higher education)";
                    case ProgrammeLevel.FoundationDegree:
                        return "Level:5 (equivalent to foundation degree)";
                    case ProgrammeLevel.Degree:
                        return "Level:6 (equivalent to bachelor’s degree)";
                    case ProgrammeLevel.Masters:
                        return "Level:7 (equivalent to master’s degree)";
                    default:
                        return "";
                }
            }
        }
    }
}
