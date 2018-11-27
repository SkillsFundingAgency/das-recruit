using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
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
            if (value == null)
            {
                return true;
            }

            return (((string)value).AsDecimal(_numberOfDecimalPlaces) != null);
        }
    }
}
