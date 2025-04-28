using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Helpers
{
    public static class EducationLevelNumberHelper
    {
        private static string GetEducationLevelName(int? educationLevelNumber)
        {
            if (educationLevelNumber.HasValue)
            {
                switch (educationLevelNumber.Value)
                {
                    case 2:
                        return "Level 2 (GCSE)";

                    case 3:
                        return "Level 3 (A level)";

                    case 4:
                        return "Level 4 (Higher national certificate)";

                    case 5:
                        return "Level 5 (Higher national diploma)";

                    case 6:
                        return "Level 6 (Degree with honours)";

                    case 7:
                        return "Level 7 (Master's degree)";
                }
            }
            return null;
        }

        public static string GetEducationLevelNameOrDefault(int? educationLevelNumber, ApprenticeshipLevel level)
            => GetEducationLevelName(educationLevelNumber) ?? $"Level {(int)level} ({level})";
        
        private static string GetTableFormatEducationLevelName(int? educationLevelNumber)
        {
            if (educationLevelNumber.HasValue)
            {
                switch (educationLevelNumber.Value)
                {
                    case 2:
                        return "Level: 2 (GCSE)";

                    case 3:
                        return "Level: 3 (A level)";

                    case 4:
                        return "Level: 4 (Higher national certificate)";

                    case 5:
                        return "Level: 5 (Higher national diploma)";

                    case 6:
                        return "Level: 6 (Degree with honours)";

                    case 7:
                        return "Level: 7 (Master's degree)";
                }
            }
            return null;
        }
        
        public static string GetTableFormatEducationLevelNameOrDefault(int? educationLevelNumber, ApprenticeshipLevel level)
            => GetTableFormatEducationLevelName(educationLevelNumber) ?? $"Level: {(int)level} ({level})";
    }
}
