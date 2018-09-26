using System;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class TitlePopularityService : IGetTitlePopularity
    {
        private readonly IGetVacancyTitlesProvider _titlesProvider;

        public TitlePopularityService(IGetVacancyTitlesProvider titlesProvider)
        {
            _titlesProvider = titlesProvider;
        }

        public async Task<int> GetTitlePopularityAsync(string larsCode, string title)
        {
            var titles = await _titlesProvider.GetVacancyTitlesAsync(larsCode);
            var totalCount = titles.Count;

            if (totalCount == 0 || !titles.Select(t => t.ToLower()).Contains(title.ToLower()))
                return 0;

            var matchedTitleGroup = titles.GroupBy(t => t)
                                            .FirstOrDefault(g => g.Key.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (matchedTitleGroup != null)
            {
                var calculatedPopularityPercentage = (100 * matchedTitleGroup.Count()) / totalCount;
                return calculatedPopularityPercentage;
            }

            return 0;
        }
    }
}