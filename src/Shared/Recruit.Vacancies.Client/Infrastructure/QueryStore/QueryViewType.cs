namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal struct QueryViewType
    {
        public string TypeName { get; }
        private readonly string _idFormatString;

        private QueryViewType(string name, string formatString)
        {
            TypeName = name;
            _idFormatString = formatString;
        }

        public string GetIdValue(object[] args)
        {
            return string.Format(_idFormatString, args);
        }

        public string GetIdValue(string value)
        {
            return string.Format(_idFormatString, value);
        }

        public string GetIdValue(long value)
        {
            return string.Format(_idFormatString, value);
        }

        public string GetIdValue()
        {
            return _idFormatString;
        }

        public static QueryViewType EmployerDashboard => new QueryViewType(nameof(EmployerDashboard), "EmployerDashboard_{0}");
        public static QueryViewType ProviderDashboard => new QueryViewType(nameof(ProviderDashboard), "ProviderDashboard_{0}");
        public static QueryViewType ProviderTraineeshipDashboard => new QueryViewType(nameof(ProviderTraineeshipDashboard), "ProviderTraineeshipDashboard_{0}");
        public static QueryViewType EditVacancyInfo => new QueryViewType(nameof(EditVacancyInfo), "EditVacancyInfo_{0}");
        public static QueryViewType LiveVacancy => new QueryViewType(nameof(LiveVacancy), "LiveVacancy_{0}");
        public static QueryViewType VacancyApplications => new QueryViewType(nameof(VacancyApplications), "VacancyApplications_{0}");
        public static QueryViewType QaDashboard => new QueryViewType(nameof(QaDashboard), "QaDashboard");
        public static QueryViewType ClosedVacancy => new QueryViewType(nameof(ClosedVacancy), "ClosedVacancy_{0}");
        public static QueryViewType VacancyAnalyticsSummary => new QueryViewType(nameof(VacancyAnalyticsSummary), "VacancyAnalyticsSummary_{0}");
        public static QueryViewType VacancyAnalyticsSummaryV2 => new QueryViewType(nameof(VacancyAnalyticsSummaryV2), "VacancyAnalyticsSummaryV2_{0}");
        public static QueryViewType BlockedEmployerOrganisations => new QueryViewType(nameof(BlockedEmployerOrganisations), "BlockedEmployerOrganisations");
        public static QueryViewType BlockedProviderOrganisations => new QueryViewType(nameof(BlockedProviderOrganisations), "BlockedProviderOrganisations");
    }
}