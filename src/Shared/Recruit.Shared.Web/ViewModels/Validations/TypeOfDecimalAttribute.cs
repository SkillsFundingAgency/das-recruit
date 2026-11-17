using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Shared.Web.ViewModels.Validations;

using System.ComponentModel.DataAnnotations;

public class TypeOfDecimalAttribute(int numberOfDecimalPlaces) : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null)
        {
            return true;
        }

        return (((string)value).AsDecimal(numberOfDecimalPlaces) != null);
    }
}