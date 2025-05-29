using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider
{
    public class ProviderDashboardSummary
    {
        public int Closed { get; set; }
        public int Draft { get; set; }
        public int Review { get; set; }
        public int Referred { get; set; }
        public int Live { get; set; }
        public int Submitted { get; set; }
        public bool HasVacancies => Closed + Draft + Review + Referred + Live + Submitted> 0;
        public bool HasOneVacancy => Closed + Draft + Review + Referred + Live + Submitted == 1;
        public int NumberOfVacancies => Closed + Draft + Review + Referred + Live + Submitted;
        public int NumberClosingSoon { get; set; }
        public int NumberOfNewApplications { get; set; }
        public int NumberOfEmployerReviewedApplications { get; set; }
        public int NumberOfSuccessfulApplications { get; set; }
        public int NumberOfUnsuccessfulApplications { get; set; }
        public int NumberClosingSoonWithNoApplications { get; set; }

        public bool HasApplications =>
            NumberOfNewApplications + NumberOfSuccessfulApplications + NumberOfUnsuccessfulApplications > 0;

        public IEnumerable<ProviderDashboardTransferredVacancy> TransferredVacancies { get; set; }
    }
}