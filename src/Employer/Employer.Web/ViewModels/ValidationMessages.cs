namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public static class ValidationMessages
    {
        public static class LocationPreferenceMessages
        {
            public const string SelectionRequired = "Select where the apprentice will work";
        }
        public static class EmployerSelectionValidationMessages
        {
            public const string EmployerSelectionRequired = "You must select an organisation.";
        }

        public static class TrainingFirstVacancyValidationMessages
        {
            public const string HasFoundTraining = "Select whether you’ve already found apprenticeship training";
        }

        public static class TrainingProviderValidationMessages
        {
            public const string IsTrainingProviderSelectedNotNull = "Select yes if you’ve found a training provider";
            public const string UkprnNotEmpty = "You must provide a UKPRN";
            public const string UkprnIsValid = "UKPRN is not recognised";
            public const string TrainingProviderSearchNotEmpty = "Please select a training provider";
        }
    }
}