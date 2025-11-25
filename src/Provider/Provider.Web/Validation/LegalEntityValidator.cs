using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Provider.Web.Validation;

public class LegalEntityValidator: AbstractValidator<Vacancy>
{
    public LegalEntityValidator(ILegalEntityAgreementService legalEntityAgreementService)
    {
        // Validates that a legal entity exists in the Employer's account
        // This does not assess whether an agreement has been signed or not
        When(x => !string.IsNullOrWhiteSpace(x.AccountLegalEntityPublicHashedId), () =>
        {
            RuleFor(x => x.AccountLegalEntityPublicHashedId).CustomAsync(async (value, context, _) =>
            {
                var legalEntity = await legalEntityAgreementService.GetLegalEntityAsync(context.InstanceToValidate.EmployerAccountId, value);
                if (legalEntity is null)
                {
                    context.AddFailure(nameof(context.InstanceToValidate.AccountLegalEntityPublicHashedId), "Enter a valid employer name");
                }
            });
        });
    }
}