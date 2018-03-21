namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public static class ValidationMessages
    {

        public static class TitleValidationMessages
        {
            public static class Required
            {
                public const string Title = "Enter the title of the vacancy";
            }

            public static class StringLength
            {
                public const string Title = "The title must not be more than {1} characters";
            }

            public static class FreeText
            {
                public const string Title = "The title contains some invalid characters";
            }
        }

        public static class EmployerEditModelValidationMessages
        {
            public static class Required
            {
                public const string SelectedOrganisationId = "You must select one organisation";
                public const string AddressLine1 = "You must enter an address";
                public const string Postcode = "Enter the postcode";
            }

            public static class StringLength
            {
                public const string AddressLine1 = "The first line of the address must not be more than {1} characters";
                public const string AddressLine2 = "The second line of the address must not be more than {1} characters";
                public const string AddressLine3 = "The third line of the address must not be more than {1} characters";
                public const string AddressLine4 = "The fourth line of the address must not be more than {1} characters";
            }

            public static class FreeText
            {
                public const string AddressLine1 = "The first line of the address contains some invalid characters";
                public const string AddressLine2 = "The second line of the address contains some invalid characters";
                public const string AddressLine3 = "The third line of the address contains some invalid characters";
                public const string AddressLine4 = "The fourth line of the address contains some invalid characters";
            }

            public static class PostcodeAttribute
            {
                public const string Postcode = "'Postcode' is not a valid format";
            }
        }

        public class ShortDescriptionValidationMessages
        {
            public static class Required
            {
                public const string NumberOfPositions = "Enter the number of positions for this vacancy";
                public const string ShortDescription = "Enter the brief overview of the vacancy";
            }

            public static class TypeOfInteger
            {
                public const string NumberOfPositions = "Enter the number of positions for this vacancy";
            }

            public static class Range
            {
                public const string NumberOfPositions = "The number of positions must be greater than zero";
            }

            public static class StringLength
            {
                public const string ShortDescription = "The overview of the role must be between {2} and {1} characters";
            }

            public static class FreeText
            {
                public const string ShortDescription = "The overview of the vacancy contains some invalid characters";
            }
        }

        public static class TrainingValidationMessages
        {
            public static class Required
            {
                public const string ClosingDate = "Enter the closing date for applications";
                public const string StartDate = "Enter the possible start date";
                public const string SelectedProgrammeId = "Select apprenticeship training";
            }

            public static class TypeOfDate
            {
                public const string ClosingDate = "The closing date format should be dd/mm/yyyy";
                public const string StartDate = "The start date format should be dd/mm/yyyy";
            }
        }

        public static class WageValidationMessages
        {
            public static class Required
            {
                public const string Duration = "Enter the expected duaration";
                public const string WorkingWeekDescription = "Enter the working week";
                public const string WeeklyHours = "Enter the hours per week";

            }

            public static class TypeOfInteger
            {
                public const string Duration = "The field expected duration must be a number";
            }

            public static class FreeText
            {
                public const string WorkingWeekDescription = "The working week contains some invalid characters";
                public const string WageAdditionalInformation = "Additional salary information contains some invalid characters";
            }

            public static class StringLength
            {
                public const string WorkingWeekDescription = "The working week must not be more than {1} characters";
                public const string WageAdditionalInformation = "Additional salary information must not be more than {1} characters";
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
    }
}
