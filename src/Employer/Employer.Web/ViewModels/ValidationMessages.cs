namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ValidationMessages
    {

        public class TitleValidationMessages
        {
            public class Required
            {
                public const string Title = "Enter the title";
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
                public const string SelectedOrganisationId = "You must select a legal entity";
                public const string AddressLine1 = "Enter your first line of address";
                public const string Postcode = "Enter your postcode";
            }

            public class StringLength
            {
                public const string AddressLine1 = "First line of address must not be more than {1} characters";
                public const string AddressLine2 = "Second line of address must not be more than {1} characters";
                public const string AddressLine3 = "Third line of address must not be more than {1} characters";
                public const string AddressLine4 = "Fourth line of address must not be more than {1} characters";
            }

            public class FreeText
            {
                public const string AddressLine1 = "First line of address contains some invalid characters";
                public const string AddressLine2 = "Second line of address contains some invalid characters";
                public const string AddressLine3 = "Third line of address contains some invalid characters";
                public const string AddressLine4 = "Fourth line of address contains some invalid characters";
            }

            public class PostcodeAttribute
            {
                public const string Postcode = "'postcode' is not a valid format";
            }
        }

        public class ShortDescriptionValidationMessages
        {
            public class Required
            {
                public const string NumberOfPositions = "Enter the number of positions for this vacancy";
                public const string ShortDescription = "Enter the brief overview of the role";
            }

            public class Range
            {
                public const string NumberOfPositions = "The number of positions must be greater than zero";
            }

            public class StringLength
            {
                public const string ShortDescription = "The brief overview of the role must be between {2} and {1} characters";
            }

            public class FreeText
            {
                public const string ShortDescription = "The brief overview of the role contains some invalid characters";
            }
        }
    }
}
