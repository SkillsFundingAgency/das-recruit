using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;

namespace Esfa.Recruit.Shared.Web.Helpers
{
    public static class VacancyFilterHeadingHelper
    {
        public static string GetFilterHeading(int totalVacancies, FilteringOptions filteringOption, string searchTerm)
        {
            var vacancyWord = "advert".ToQuantity(totalVacancies, ShowQuantityAs.None);

            var words = new List<string>();

            words.Add(totalVacancies.ToString());

            switch(filteringOption)
            {
                case FilteringOptions.All:
                    words.Add(vacancyWord);
                    break;
                case FilteringOptions.Closed:
                case FilteringOptions.Draft:
                case FilteringOptions.Live:
                case FilteringOptions.Referred:                
                    words.Add(filteringOption.GetDisplayName().ToLowerInvariant());
                    words.Add(vacancyWord);
                    break;
                default:
                    words.Add(vacancyWord);
                    words.Add(filteringOption.GetDisplayName().ToLowerInvariant());
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
