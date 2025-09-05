using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammeProvider : IApprenticeshipProgrammeProvider
    {
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;
        private readonly IOuterApiClient _outerApiClient;
        private readonly IFeature _feature;

        public ApprenticeshipProgrammeProvider(ICache cache, ITimeProvider timeProvider, IOuterApiClient outerApiClient, IFeature feature, IConfiguration configuration)
        {
            _cache = cache;
            _timeProvider = timeProvider;
            _outerApiClient = outerApiClient;
            _feature = feature;
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

        public async Task<IEnumerable<IApprenticeshipProgramme>> GetApprenticeshipProgrammesAsync(bool includeExpired = false, int? ukprn = null)
        {
            var queryItem = await GetApprenticeshipProgrammes(ukprn);
            return includeExpired ? 
                queryItem.Data : 
                queryItem.Data.Where(x =>x.IsActive || (x.ApprenticeshipType == TrainingType.Foundation && IsStandardActive(x.EffectiveTo,x.LastDateStarts)));
        }

        private async Task<ApprenticeshipProgrammes> GetApprenticeshipProgrammes(int? ukprn)
        {
            var result = await _outerApiClient.Get<GetTrainingProgrammesResponse>(new GetTrainingProgrammesRequest(ukprn));
            return new ApprenticeshipProgrammes
            {
                Data = result.TrainingProgrammes.Select(c => (ApprenticeshipProgramme)c).ToList(),
            };
        }
        
        private bool IsStandardActive(DateTime? effectiveTo, DateTime? lastDateStarts)
        {
            return (!effectiveTo.HasValue || effectiveTo.Value.Date >= DateTime.UtcNow.Date)
                   && (!lastDateStarts.HasValue || lastDateStarts.Value.Date >= DateTime.UtcNow.Date);
        }
    }
}