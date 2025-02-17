using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammeProvider : IApprenticeshipProgrammeProvider
    {
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;
        private readonly IOuterApiClient _outerApiClient;

        public ApprenticeshipProgrammeProvider(ICache cache, ITimeProvider timeProvider, IOuterApiClient outerApiClient)
        {
            _cache = cache;
            _timeProvider = timeProvider;
            _outerApiClient = outerApiClient;
        }

        public async Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId)
        {
            var apprenticeships = await GetApprenticeshipProgrammesAsync(true);

            return apprenticeships?.SingleOrDefault(x => x.Id == programmeId);
        }

        public async Task<ApprenticeshipStandard> GetApprenticeshipStandardVacancyPreviewData(int programmedId)
        {
            return await _outerApiClient.Get<GetVacancyPreviewApiResponse>(new GetVacancyPreviewApiRequest(programmedId));
        }

        public async Task<IEnumerable<IApprenticeshipProgramme>> GetApprenticeshipProgrammesAsync(bool includeExpired = false)
        {
            var queryItem = await GetApprenticeshipProgrammes();
            return includeExpired ? 
                queryItem.Data : 
                queryItem.Data.Where(x => x.IsActive);
        }

        private Task<ApprenticeshipProgrammes> GetApprenticeshipProgrammes()
        {
            return _cache.CacheAsideAsync(CacheKeys.ApprenticeshipProgrammes,
                _timeProvider.NextDay6am,
                async () =>
                {
                    var result = await _outerApiClient.Get<GetTrainingProgrammesResponse>(new GetTrainingProgrammesRequest());
                    return new ApprenticeshipProgrammes
                    {
                        Data = result.TrainingProgrammes.Select(c => (ApprenticeshipProgramme)c).ToList(),
                        
                    };
                });
        }
    }
}