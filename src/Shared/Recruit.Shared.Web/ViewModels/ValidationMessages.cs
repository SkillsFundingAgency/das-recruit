namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public static class ValidationMessages
    {
        public class PositionValidationMessages
        {
            public static class TypeOfInteger
            {
                public const string NumberOfPositions = "Enter the number of positions";
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

        public static class DurationValidationMessages
        {
            public static class TypeOfInteger
            {
                public const string Duration = "Expected duration must be a number";
            }

            public static class TypeOfDecimal
            {
                public const string WeeklyHours = "Hours per week must be a number";
            }
        }

        public static class WageValidationMessages
        {
            public static class TypeOfMoney
            {
                public const string FixedWageYearlyAmount = "Fixed wage must be a number";
            }
        }

        public static class DeleteVacancyConfirmationMessages
        {
            public const string SelectionRequired = "Select yes if you want to delete this advert";
        }

        public static class CloseVacancyConfirmationMessages
        {
            public const string SelectionRequired = "Select yes if you want to close this advert on Find an apprenticeship";
        }

        public static class CloneVacancyConfirmationMessages
        {
            public const string SelectionRequired = "Select whether the new advert has the same closing date and start date";
        }

        public static class CreateVacancyOptionsConfirmationMessages
        {
            public const string SelectionRequired = "Select whether you want to create a new advert or clone one of your existing adverts";
        }

        public static class UnsubscribeNotificationsConfirmationMessages
        {
            public const string SelectionRequired = "Select yes if you want to stop receiving emails about adverts and applications";
        }


        public static class QualificationsConfirmationMessages
        {
            public const string SelectionRequired = "Select if you want to add any qualification requirements";
        }
    }
}