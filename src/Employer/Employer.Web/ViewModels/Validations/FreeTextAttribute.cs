namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    using System.ComponentModel.DataAnnotations;

    public class FreeTextAttribute : RegularExpressionAttribute
    {
        private const string RegexFreeTextWhitelist = @"^[a-zA-Z0-9\u0080-\uFFA7?$@#()""'!,+\-=_:;.&€£*%\s\/\[\]]*$";

        private const string Message = "{0} contains invalid characters.";

        public FreeTextAttribute() : base(RegexFreeTextWhitelist)
        {
            ErrorMessage = Message;
        }
    }
}
