﻿using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class EmployerVacancyOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ITrainingProviderSummaryProvider _trainingProviderSummaryProvider;
        private readonly IEmployerVacancyClient _employerVacancyClient;

        public EmployerVacancyOrchestrator(IRecruitVacancyClient vacancyClient, ITrainingProviderSummaryProvider trainingProviderSummaryProvider, 
            IEmployerVacancyClient employerVacancyClient)
        {
            _vacancyClient = vacancyClient;
            _trainingProviderSummaryProvider = trainingProviderSummaryProvider;
            _employerVacancyClient = employerVacancyClient;
        }

        public async Task<bool> HasNoVacancies(string employerAccountId)
        {
            var vacancyCount = await _employerVacancyClient.GetVacancyCount(employerAccountId, null, null);
            return vacancyCount == 0;
        }

        public async Task<TrainingProviderSummary> GetProviderUkprn(string ukprn)
        {
            if (long.TryParse(ukprn, out long validUkprn) == false)
                return null;
            var providers = await _trainingProviderSummaryProvider.FindAllAsync();
            return providers.SingleOrDefault(p => p.Ukprn == validUkprn);
        }

        public async Task<IApprenticeshipProgramme> GetProgrammeId(string programmeId)
        {
            if (string.IsNullOrWhiteSpace(programmeId))
                return null;
            var programmes = await _vacancyClient.GetActiveApprenticeshipProgrammesAsync();
            return programmes.SingleOrDefault(p => p.Id == programmeId);
        }
    }
}