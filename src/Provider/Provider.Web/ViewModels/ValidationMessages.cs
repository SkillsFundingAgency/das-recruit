using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public static class ValidationMessages
    {
        public static class EmployerSelectionMessages
        {
            public const string EmployerMustBeSelectedMessage = "You must select an employer";
        }
        public static class EmployerNameValidationMessages
        {
            public const string EmployerNameRequired = "You must select one option.";
        }
        public static class LocationPreferenceMessages
        {
            public const string SelectionRequired = "You must select one option.";
        }
    }
}