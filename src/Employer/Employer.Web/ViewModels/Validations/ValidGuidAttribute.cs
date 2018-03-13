namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ValidGuidAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            //Ensuring guid is not 00000000-0000-0000-0000-000000000000
            return !Equals((Guid)value, default(Guid));
        }
    }
}
