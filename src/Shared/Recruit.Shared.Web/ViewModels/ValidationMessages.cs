namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public static class ValidationMessages
    {
        public class TitleValidationMessages
        {
            public static class TypeOfInteger
            {
                public const string NumberOfPositions = "You must state the number of positions for this vacancy";
            }
        }

        public static class DateValidationMessages
        {
            public static class TypeOfDate
            {
                public const string ClosingDate = "Application closing date format should be dd/mm/yyyy";
                public const string StartDate = "Possible start date format should be dd/mm/yyyy";
            }
        }

        public static class WageValidationMessages
        {
            public static class TypeOfInteger
            {
                public const string Duration = "Expected duration must be a number";
            }

            public static class TypeOfDecimal
            {
                public const string WeeklyHours = "Hours per week must be a number";
            }

            public static class TypeOfMoney
            {
                public const string FixedWageYearlyAmount = "Fixed wage must be a number";
            }
        }

        public static class DeleteVacancyConfirmationMessages
        {
            public const string SelectionRequired = "You must select one option";
        }

        public static class CloseVacancyConfirmationMessages
        {
            public const string SelectionRequired = "You must select one option";
        }

        public static class CloneVacancyConfirmationMessages
        {
            public const string SelectionRequired = "You must select one option";
        }

        public static class CreateVacancyOptionsConfirmationMessages
        {
            public const string SelectionRequired = "You must select either 'Create new vacancy' or clone one of your existing vacancies.";
        }

        public static class LevyDeclarationConfirmationMessages
        {
            public const string SelectionRequired = "You must select one option.";
        }

        public static class UnsubscribeNotificationsConfirmationMessages
        {
            public const string SelectionRequired =  "Please confirm if you’d like to unsubscribe";
        }
    }
}