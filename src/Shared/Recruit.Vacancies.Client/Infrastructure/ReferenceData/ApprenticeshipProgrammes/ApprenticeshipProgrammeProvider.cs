using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammeProvider(IOuterApiClient outerApiClient) : IApprenticeshipProgrammeProvider
    {
        private const int DummyStandardProgrammeId = 999999;

        public async Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId, int? ukprn = null)
        {
            var apprenticeships = await GetApprenticeshipProgrammesAsync(true, ukprn);

            return apprenticeships?.SingleOrDefault(x => x.Id == programmeId);
        }

        public async Task<ApprenticeshipStandard> GetApprenticeshipStandardVacancyPreviewData(int programmedId)
        {
            if (programmedId.Equals(DummyStandardProgrammeId)) return GetDummyStandard();

            return await outerApiClient.Get<GetVacancyPreviewApiResponse>(new GetVacancyPreviewApiRequest(programmedId));
        }

        public async Task<IEnumerable<IApprenticeshipProgramme>> GetApprenticeshipProgrammesAsync(bool includeExpired = false, int? ukprn = null)
        {
            var queryItem = await GetApprenticeshipProgrammes(ukprn);
            return includeExpired ?
                queryItem.Data :
                queryItem.Data.Where(x => x.IsActive || (x.ApprenticeshipType == TrainingType.Foundation && IsStandardActive(x.EffectiveTo, x.LastDateStarts)));
        }

        private async Task<ApprenticeshipProgrammes> GetApprenticeshipProgrammes(int? ukprn)
        {
            var result = await outerApiClient.Get<GetTrainingProgrammesResponse>(new GetTrainingProgrammesRequest(ukprn));
            var trainingProgrammes = result.TrainingProgrammes.Select(c => (ApprenticeshipProgramme) c).ToList();
            trainingProgrammes.Add(GetDummyProgramme());
            return new ApprenticeshipProgrammes
            {
                Data = trainingProgrammes
            };
        }
        
        private bool IsStandardActive(DateTime? effectiveTo, DateTime? lastDateStarts)
        {
            return (!effectiveTo.HasValue || effectiveTo.Value.Date >= DateTime.UtcNow.Date)
                   && (!lastDateStarts.HasValue || lastDateStarts.Value.Date >= DateTime.UtcNow.Date);
        }

        private static ApprenticeshipProgramme GetDummyProgramme() =>
            new()
            {
                Id = DummyStandardProgrammeId.ToString(),
                Title = "To be confirmed",
                IsActive = true,
                ApprenticeshipType = TrainingType.Standard,
                ApprenticeshipLevel = ApprenticeshipLevel.Unknown,
                EffectiveTo = DateTime.UtcNow.AddYears(1),
                LastDateStarts = DateTime.UtcNow.AddYears(1)
            };

        private static ApprenticeshipStandard GetDummyStandard() =>
            new()
            {
                Title = "To be confirmed",
                ApprenticeshipType = TrainingType.Standard,
                ApprenticeshipLevel = ApprenticeshipLevel.Unknown,
            };
    }
}