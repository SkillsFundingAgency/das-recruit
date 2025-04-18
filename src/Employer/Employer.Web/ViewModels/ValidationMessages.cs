namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public static class ValidationMessages
    {
        public static class LocationPreferenceMessages
        {
            public const string SelectionRequired = "Select where the apprentice will work";
        }

        public static class MultipleLocationMessages
        {
            public const string SelectionRequired = "Select where this apprenticeship is available";
        }
        public static class EmployerSelectionValidationMessages
        {
            public const string EmployerSelectionRequired = "Select the organisation this advert is for";
        }

        public static class TrainingFirstVacancyValidationMessages
        {
            public const string HasFoundTraining = "Select whether you've already found apprenticeship training";
        }

        public static class TrainingProviderValidationMessages
        {
            public const string UkprnNotEmpty = "You must provide a UKPRN";
            public const string UkprnIsValid = "UKPRN is not recognised";
            public const string TrainingProviderSearchNotEmpty = "You must enter a training provider or UKPRN to continue";
        }
    }
}