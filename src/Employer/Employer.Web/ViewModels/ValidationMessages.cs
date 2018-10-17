namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public static class ValidationMessages
    {
        public class TitleValidationMessages
        {
            public static class TypeOfInteger
            {
                public const string NumberOfPositions = "Enter the number of positions for this vacancy";
            }
        }

        public static class DateValidationMessages
        {
            public static class TypeOfDate
            {
                public const string ClosingDate = "The closing date format should be dd/mm/yyyy";
                public const string StartDate = "The start date format should be dd/mm/yyyy";
            }
        }

        public static class TrainingProviderValidationMessages
        {
            public static class TypeOfUkprn
            {
                public const string UkprnFormat = "The UKPRN you have entered is not recognised";
            }
        }

        public static class WageValidationMessages
        {
            public static class TypeOfInteger
            {
                public const string Duration = "The field expected duration must be a number";
            }

            public static class TypeOfDecimal
            {
                public const string WeeklyHours = "The field paid hours per week must be a number";
            }

            public static class TypeOfMoney
            {
                public const string FixedWageYearlyAmount = "The field wage must be a number";
            }
        }

        public static class DeleteVacancyConfirmationMessages
        {
            public const string SelectionRequired = "You must select one option.";
        }

        public static class CloseVacancyConfirmationMessages
        {
            public const string SelectionRequired = "You must select one option.";
        }

        public static class CreateVacancyOptionsConfirmationMessages
        {
            public const string SelectionRequired = "You must select either 'Create new vacancy' or one of your existing vacancies.";
        }
    }
}
