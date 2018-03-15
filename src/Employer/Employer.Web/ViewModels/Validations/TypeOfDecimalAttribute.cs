namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class TypeOfDecimalAttribute : ValidationAttribute
    {
        private readonly int _numberOfDecimalPlaces;

        public TypeOfDecimalAttribute(int numberOfDecimalPlaces)
        {
            _numberOfDecimalPlaces = numberOfDecimalPlaces;
        }

        public override bool IsValid(object value)
        {
            if (decimal.TryParse((string) value, out var d))
            {
                if (decimal.Round(d, _numberOfDecimalPlaces, MidpointRounding.AwayFromZero) == d)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
