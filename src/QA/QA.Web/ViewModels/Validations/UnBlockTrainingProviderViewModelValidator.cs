using Esfa.Recruit.QA.Web.ViewModels.ManageProvider;
using FluentValidation;

namespace Esfa.Recruit.Qa.Web.ViewModels.Validations
{
    public class UnBlockTrainingProviderViewModelValidator : AbstractValidator<UnBlockTrainingProviderEditModel>
    {
        public const string SelectionRequired = "Please select an option to confirm";
        public UnBlockTrainingProviderViewModelValidator()
        {
            RuleFor(x => x.RestoreAccess)
                .NotNull()
                .WithMessage(SelectionRequired);

        }
    }
}
