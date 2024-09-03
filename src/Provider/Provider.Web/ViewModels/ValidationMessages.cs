namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public static class ValidationMessages
    {
        public static class EmployerSelectionMessages
        {
            public const string EmployerMustBeSelectedMessage = "You must select an organisation";
        }

        public static class EmployerSelectionValidationMessages
        {
            public const string EmployerSelectionRequired = "You must select an employer";
        }

        public static class LocationPreferenceMessages
        {
            public const string SelectionRequired = "You must select a work address";
        }

        public static class TraineeshipSectorValidationMessages
        {
            public const string SelectionRequired = "Select a sector";
        }

        public static class TrainingFirstVacancyValidationMessages
        {
            public const string HasFoundTraining = "Please select an option to continue.";
        }
        public static class QualificationsConfirmationMessages
        {
            public const string SelectionRequired = "Select if you want to add any qualification requirements";
        }
    }
}