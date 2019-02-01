using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public partial class VacancyClient : IProviderVacancyClient
    {              
        public async Task<Guid> CreateVacancyAsync(string employerAccountId, string employerName,
            long ukprn, string title, int numberOfPositions, VacancyUser user)
        {
            var vacancyId = GenerateVacancyId();

            var command = new CreateProviderOwnedVacancyCommand
            {
                VacancyId = vacancyId,
                User = user,
                UserType = UserType.Provider,
                EmployerAccountId = employerAccountId,
                EmployerName = employerName,
                Ukprn = ukprn,           
                Origin = SourceOrigin.ProviderWeb,
                Title = title,
                NumberOfPositions = numberOfPositions
            };

            await _messaging.SendCommandAsync(command);

            return vacancyId;
        }

        public Task<ProviderDashboard> GetDashboardAsync(long ukprn)
        {
            return _reader.GetProviderDashboardAsync(ukprn);
        }

        public Task GenerateDashboard(long ukprn)
        {
            return _providerDashboardService.ReBuildDashboardAsync(ukprn);
        }

        public Task<ProviderEditVacancyInfo> GetProviderEditVacancyInfoAsync(long ukprn)
        {
            return _reader.GetProviderVacancyDataAsync(ukprn);
        }

        public Task<EmployerInfo> GetProviderEmployerVacancyDataAsync(long ukprn, string employerAccountId)
        {
            return _reader.GetProviderEmployerVacancyDataAsync(ukprn, employerAccountId);
        }

        public Task SetupProviderAsync(long ukprn)
        {
            var command = new SetupProviderCommand(ukprn);

            return _messaging.SendCommandAsync(command);
        }

        public Task SubmitVacancyAsync(Guid vacancyId, VacancyUser user)
        {
            var command = new SubmitVacancyCommand(vacancyId, user);

            return _messaging.SendCommandAsync(command);
        }
    }
}