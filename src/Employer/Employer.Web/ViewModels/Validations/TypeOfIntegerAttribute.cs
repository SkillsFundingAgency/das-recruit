namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class TypeOfIntegerAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return (int.TryParse((string)value, out var i));
        }
    }
}
