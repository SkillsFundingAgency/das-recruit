using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class ProgrammeLevelExtensions
    {
        public static string ToDescription(this ProgrammeLevel level)
        {
            switch (level)
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
