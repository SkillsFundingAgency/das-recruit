namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public static class ValidationMessages
    {
        public static class LocationPreferenceMessages
        {
            public const string SelectionRequired = "You must select a work address";
        }
        public static class EmployerSelectionValidationMessages
        {
            public const string EmployerSelectionRequired = "You must select an organisation.";
        }

        public static class TrainingFirstVacancyValidationMessages
        {
            public const string HasFoundTraining = "Please select an option to continue.";
        }

        public static class TrainingProviderValidationMessages
        {
            public const string IsTrainingProviderSelectedNotNull = "Please select an option to continue";
            public const string UkprnNotEmpty = "You must provide a UKPRN";
            public const string UkprnIsValid = "UKPRN is not recognised";
            public const string TrainingProviderSearchNotEmpty = "Please select a training provider";
        }
    }
}