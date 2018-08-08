namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal struct QueryViewType
    {
        public string TypeName { get; private set; }
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

        public string GetIdValue()
        {
            return _idFormatString;
        }

        public static QueryViewType Dashboard => new QueryViewType("Dashboard", "Dashboard_{0}");
        public static QueryViewType EditVacancyInfo => new QueryViewType("EditVacancyInfo", "EditVacancyInfo_{0}");
        public static QueryViewType ApprenticeshipProgrammes => new QueryViewType("ApprenticeshipProgrammes", "ApprenticeshipProgrammes");
        public static QueryViewType LiveVacancy => new QueryViewType("LiveVacancy", "LiveVacancy_{0}");
        public static QueryViewType VacancyApplications => new QueryViewType("VacancyApplications", "VacancyApplications_{0}");
        public static QueryViewType QaDashboard => new QueryViewType("QaDashboard", "QaDashboard");
    }
}