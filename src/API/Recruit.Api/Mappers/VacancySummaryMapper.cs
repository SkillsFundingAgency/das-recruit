using Microsoft.Extensions.Options;
using SFA.DAS.Recruit.Api.Configuration;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Services;
using ApplicationMethod = Esfa.Recruit.Vacancies.Client.Domain.Entities.ApplicationMethod;
using VacancyStatus = Esfa.Recruit.Vacancies.Client.Domain.Entities.VacancyStatus;

namespace SFA.DAS.Recruit.Api.Mappers
{
    public class VacancySummaryMapper : IVacancySummaryMapper
    {
        private readonly RecruitConfiguration _recruitConfig;

        public VacancySummaryMapper(IOptions<RecruitConfiguration> recruitConfig)
        {
            _recruitConfig = recruitConfig.Value;
        }

        public VacancySummary MapFromVacancySummaryProjection(Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary vsp, bool isForProviderOwnedVacancies)
        {
            var raaManageVacancyFormattedUrl = isForProviderOwnedVacancies
                                                ? _recruitConfig.ProviderRecruitAnApprenticeManageVacancyFormattedUrl
                                                : _recruitConfig.EmployerRecruitAnApprenticeManageVacancyFormattedUrl;

            var shouldAssignApplicationStats = vsp.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship
                                                && (vsp.Status == VacancyStatus.Live || vsp.Status == VacancyStatus.Closed);
            return new VacancySummary
            {
                EmployerAccountId = vsp.EmployerAccountId,
                Title = vsp.Title,
                VacancyReference = vsp.VacancyReference,
                LegalEntityName = vsp.LegalEntityName,
                EmployerName = vsp.EmployerName,
                Ukprn = vsp.Ukprn,
                CreatedDate = vsp.CreatedDate,
                Status = vsp.Status.ToString(),
                ClosingDate = vsp.ClosingDate,
                ClosedDate = vsp.ClosedDate,
                Duration = vsp.Duration,
                DurationUnit = vsp.DurationUnit.ToString(),
                ApplicationMethod = vsp.ApplicationMethod?.ToString(),
                ProgrammeId = vsp.ProgrammeId,
                StartDate = vsp.StartDate,
                TrainingTitle = vsp.TrainingTitle,
                TrainingType = vsp.TrainingType.ToString(),
                TrainingLevel = vsp.TrainingLevel.ToString(),
                NoOfNewApplications = shouldAssignApplicationStats ? vsp.NoOfNewApplications : default(int?),
                NoOfSuccessfulApplications = shouldAssignApplicationStats ? vsp.NoOfSuccessfulApplications : default(int?),
                NoOfUnsuccessfulApplications = shouldAssignApplicationStats ? vsp.NoOfUnsuccessfulApplications : default(int?),

                FaaVacancyDetailUrl = vsp.VacancyReference.HasValue
                                        ? $"{_recruitConfig.FindAnApprenticeshipDetailPrefixUrl}{vsp.VacancyReference}"
                                        : null,
                RaaManageVacancyUrl = isForProviderOwnedVacancies
                                        ? string.Format(raaManageVacancyFormattedUrl, vsp.Ukprn, vsp.Id)
                                        : string.Format(raaManageVacancyFormattedUrl, vsp.EmployerAccountId, vsp.Id)
            };
        }
    }
}