using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;

namespace Esfa.Recruit.Shared.Web.Helpers
{
    public static class VacancyFilterHeadingHelper
    {
        public static string GetFilterHeading(string vacancyTerm, int totalVacancies, FilteringOptions filteringOption, string searchTerm, UserType? userType = null)
        {
            var vacancyWord = vacancyTerm.ToQuantity(totalVacancies, ShowQuantityAs.None);

            var words = new List<string> {totalVacancies.ToString()};

            switch(filteringOption)
            {
                case FilteringOptions.All:
                    words.Add(vacancyWord);
                    break;
                case FilteringOptions.Closed:
                case FilteringOptions.Draft:
                case FilteringOptions.Live:
                case FilteringOptions.Referred:                
                    words.Add(filteringOption.GetDisplayName(userType).ToLowerInvariant());
                    words.Add(vacancyWord);
                    break;
                case FilteringOptions.Submitted:
                    words.Add(vacancyWord);
                    words.Add("pending DfE review"); /* custom wording for submitted vacancies as this is needs to be in lower case
                                                      and is not the same as the display name for submitted vacancies which is "Pending DfE review" */
                    break;
                default:
                    words.Add(vacancyWord);
                    words.Add(filteringOption.GetDisplayName(userType).ToLowerInvariant());
                    break;
            }
            
            if(!string.IsNullOrWhiteSpace(searchTerm))
            {
                words.Add($"with '{searchTerm}'");
            }

            return string.Join(" ", words);
        }
    }
}
