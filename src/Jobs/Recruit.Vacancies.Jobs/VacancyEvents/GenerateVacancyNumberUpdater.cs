using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.GenerateVacancyNumber
{
    public class GenerateVacancyNumberUpdater
    {
        private readonly IVacancyClient _vacancyClient;

        public GenerateVacancyNumberUpdater(IVacancyClient client)
        {
            _vacancyClient = client;
        }

        internal Task AssignVacancyNumber(Guid vacancyId)
        {
            return _vacancyClient.AssignVacancyNumber(vacancyId);
        }
    }
}

