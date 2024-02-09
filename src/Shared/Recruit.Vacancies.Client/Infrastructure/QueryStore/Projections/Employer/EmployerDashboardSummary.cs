namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer
{
    public class EmployerDashboardSummary
    {
        public int Closed { get; set; }
        public int Draft { get; set; }
        public int Review { get; set; }
        public int Referred { get; set; }
        public int Live { get; set; }
        public int Submitted { get; set; }
        public bool HasVacancies => Closed + Draft + Review + Referred + Live + Submitted > 0;
        public bool HasOneVacancy => Closed + Draft + Review + Referred + Live + Submitted == 1;
        public int NumberOfVacancies => Closed + Draft + Review + Referred + Live + Submitted;
        public bool HasAnySharedVacancy => NumberOfAllSharedApplications > 0;
        public int NumberClosingSoon { get; set; }
        public int NumberOfNewApplications { get; set; }
        public int NumberOfSuccessfulApplications { get; set; }
        public int NumberOfUnsuccessfulApplications { get; set; }
        public int NumberClosingSoonWithNoApplications { get; set; }
        public int NumberOfSharedApplications { get; set; }
        public int NumberOfAllSharedApplications { get; set; }

        public bool HasApplications =>
            NumberOfNewApplications + NumberOfSuccessfulApplications + NumberOfUnsuccessfulApplications > 0;
    }
}