using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammeProvider(
        IOuterApiClient outerApiClient,
        ICache cache,
        ITimeProvider timeProvider) : IApprenticeshipProgrammeProvider
    {
        public async Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId, int? ukprn = null, bool includePlaceholderProgramme = false)
        {
            var apprenticeships = await GetApprenticeshipProgrammesAsync(true, ukprn, includePlaceholderProgramme);

            return apprenticeships?.SingleOrDefault(x => x.Id == programmeId);
        }

        public async Task<ApprenticeshipStandard> GetApprenticeshipStandardVacancyPreviewData(int programmedId)
        {
            if (programmedId.Equals(EsfaTestTrainingProgramme.Id)) return GetDummyStandard();

            return await outerApiClient.Get<GetVacancyPreviewApiResponse>(new GetVacancyPreviewApiRequest(programmedId));
        }

        public async Task<IEnumerable<IApprenticeshipProgramme>> GetApprenticeshipProgrammesAsync(bool includeExpired = false, int? ukprn = null, bool includePlaceholderProgramme = false)
        {
            var queryItem = await GetApprenticeshipProgrammes(ukprn, includePlaceholderProgramme);
            return includeExpired ?
                queryItem.Data :
                queryItem.Data.Where(x => x.IsActive || (x.ApprenticeshipType == TrainingType.Foundation && IsStandardActive(x.EffectiveTo, x.LastDateStarts)));
        }

        private async Task<ApprenticeshipProgrammes> GetApprenticeshipProgrammes(int? ukprn, bool includePlaceholderProgramme = false)
        {
            var trainingProviders = await cache.CacheAsideAsync(CacheKeys.ApprenticeshipProgrammes,
                timeProvider.NextDay6am,
                async () =>
                {
                    var result = await outerApiClient.Get<GetTrainingProgrammesResponse>(new GetTrainingProgrammesRequest(ukprn));
                    var trainingProgrammes = result.TrainingProgrammes.Select(c => (ApprenticeshipProgramme)c).ToList();
                    return new ApprenticeshipProgrammes
                    {
                        Data = trainingProgrammes
                    };
                });

            if (!includePlaceholderProgramme) return trainingProviders;
            
            if (trainingProviders.Data.All(tp => tp.Id != EsfaTestTrainingProgramme.Id.ToString()))
            {
                trainingProviders.Data.Add(GetDummyProgramme());
            }

            return trainingProviders;
        }
        
        private static bool IsStandardActive(DateTime? effectiveTo, DateTime? lastDateStarts) =>
            (!effectiveTo.HasValue || effectiveTo.Value.Date >= DateTime.UtcNow.Date)
            && (!lastDateStarts.HasValue || lastDateStarts.Value.Date >= DateTime.UtcNow.Date);

        private static ApprenticeshipProgramme GetDummyProgramme() =>
            new()
            {
                Id = EsfaTestTrainingProgramme.Id.ToString(),
                Title = EsfaTestTrainingProgramme.Title,
                IsActive = true,
                ApprenticeshipType = EsfaTestTrainingProgramme.ApprenticeshipType,
                ApprenticeshipLevel = EsfaTestTrainingProgramme.ApprenticeshipLevel,
                EffectiveTo = DateTime.UtcNow.AddYears(1),
                LastDateStarts = DateTime.UtcNow.AddYears(1)
            };

        private static ApprenticeshipStandard GetDummyStandard() =>
            new()
            {
                Title = EsfaTestTrainingProgramme.Title,
                ApprenticeshipType = EsfaTestTrainingProgramme.ApprenticeshipType,
                ApprenticeshipLevel = EsfaTestTrainingProgramme.ApprenticeshipLevel,
            };
    }
}