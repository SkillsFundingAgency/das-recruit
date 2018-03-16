namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal struct QueryViewType
    {
        public string TypeName { get; private set; }
        private string _idFormatString;

        private QueryViewType(string name, string formatString)
        {
            TypeName = name;
            _idFormatString = formatString;
        }

        public string GetIdValue(string[] args)
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

        public static QueryViewType Dashboard => new QueryViewType("Dashboard", "dashboard_{0}");
        public static QueryViewType EmployerVacancyData => new QueryViewType("EmployerVacancyData", "employer_{0}");
        public static QueryViewType ApprenticeshipProgrammes => new QueryViewType("ApprenticeshipProgrammes", "ApprenticeshipProgrammes");
    }
}
