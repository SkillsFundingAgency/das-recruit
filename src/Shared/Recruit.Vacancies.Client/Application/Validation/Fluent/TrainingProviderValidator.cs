using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    internal class TrainingProviderValidator : AbstractValidator<TrainingProvider>
    {
        public TrainingProviderValidator(long ruleId)
        {
            RuleFor(tp => tp.Ukprn.ToString())
                .NotEmpty()
                    .WithMessage("You must enter a training provider")
                    .WithErrorCode("101")
                .Length(8)
                    .WithMessage("The UKPRN is 8 digits")
                    .WithErrorCode("99")
                .WithRuleId(ruleId);
        }
    }
}
