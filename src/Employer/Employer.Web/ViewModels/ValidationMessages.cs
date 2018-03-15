namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ValidationMessages
    {

        public class TitleValidationMessages
        {
            public class Required
            {
                public const string Title = "Enter the title of the vacancy";
            }

            public class StringLength
            {
                public const string Title = "The title must not be more than {1} characters";
            }

            public class FreeText
            {
                public const string Title = "The title contains some invalid characters";
            }
        }

        public class EmployerEditModelValidationMessages
        {
            public class Required
            {
                public const string SelectedOrganisationId = "You must select one organisation";
                public const string AddressLine1 = "You must enter an address";
                public const string Postcode = "Enter the postcode";
            }

            public class StringLength
            {
                public const string AddressLine1 = "The first line of the address must not be more than {1} characters";
                public const string AddressLine2 = "The second line of the address must not be more than {1} characters";
                public const string AddressLine3 = "The third line of the address must not be more than {1} characters";
                public const string AddressLine4 = "The fourth line of the address must not be more than {1} characters";
            }

            public class FreeText
            {
                public const string AddressLine1 = "The first line of the address contains some invalid characters";
                public const string AddressLine2 = "The second line of the address contains some invalid characters";
                public const string AddressLine3 = "The third line of the address contains some invalid characters";
                public const string AddressLine4 = "The fourth line of the address contains some invalid characters";
            }

            public class PostcodeAttribute
            {
                public const string Postcode = "'Postcode' is not a valid format";
            }
        }

        public class ShortDescriptionValidationMessages
        {
            public class Required
            {
                public const string NumberOfPositions = "Enter the number of positions for this vacancy";
                public const string ShortDescription = "Enter the brief overview of the vacancy";
            }

            public class TypeOfInteger
            {
                public const string NumberOfPositions = "Enter the number of positions for this vacancy";
            }

            public class Range
            {
                public const string NumberOfPositions = "The number of positions must be greater than zero";
            }

            public class StringLength
            {
                public const string ShortDescription = "The overview of the role must be between {2} and {1} characters";
            }

            public class FreeText
            {
                public const string ShortDescription = "The overview of the vacancy contains some invalid characters";
            }
        }

        public class TrainingValidationMessages
        {
            public class Required
            {
                public const string ClosingDate = "Enter the closing date for applications";
                public const string StartDate = "Enter the possible start date";
                public const string SelectedProgrammeId = "Select apprenticeship training";
            }

            public class TypeOfDate
            {
                public const string ClosingDate = "The closing date format should be dd/mm/yyyy";
                public const string StartDate = "The start date format should be dd/mm/yyyy";
            }
        }

        public class WageValidationMessages
        {
            public class Required
            {
                public const string Duration = "Enter the expected duaration";
                public const string WorkingWeekDescription = "Enter the working week";
                public const string WeeklyHours = "Enter the hours per week";

            }

            public class TypeOfInteger
            {
                public const string Duration = "The field expected duration must be a number";
            }

            public class FreeText
            {
                public const string WorkingWeekDescription = "The working week contains some invalid characters";
                public const string WageAdditionalInformation = "Additional salary information contains some invalid characters";
            }

            public class StringLength
            {
                public const string WorkingWeekDescription = "The working week must not be more than {1} characters";
                public const string WageAdditionalInformation = "Additional salary information must not be more than {1} characters";
            }

            public class TypeOfDecimal
            {
                public const string WeeklyHours = "The field paid hours per week must be a number";
            }

            public class TypeOfMoneyGBP
            {
                public const string FixedWageYearlyAmount = "The field wage must be a number";
            }
        }
    }
}
