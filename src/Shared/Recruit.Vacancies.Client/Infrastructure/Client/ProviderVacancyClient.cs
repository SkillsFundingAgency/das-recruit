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
        public async Task<Guid> CreateVacancyAsync(SourceOrigin origin, 
            string employerAccountId, long ukprn, VacancyUser user, UserType userType)
        {
            var vacancyId = GenerateVacancyId();

            var command = new CreateProviderOwnedVacancyCommand
            {
                VacancyId = vacancyId,
                User = user,
                UserType = userType,
                EmployerAccountId = employerAccountId,    
                Ukprn = ukprn,           
                Origin = origin
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
            return Task.FromResult(new ProviderEditVacancyInfo{
                Employers = new List<EmployerInfo>{                    
                    {new EmployerInfo{ Id = "1234", Name = "Rogers and Federrers"  }}
                }
            });
        }

        public Task SetupProviderAsync(long ukprn)
        {
            var command = new SetupProviderCommand(ukprn);

            return _messaging.SendCommandAsync(command);
        }
    }
}