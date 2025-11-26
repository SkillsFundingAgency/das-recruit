using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Provider.Web.Validation;

public class LegalEntityExistsValidator: AbstractValidator<Vacancy>
{
    public LegalEntityExistsValidator(ILegalEntityAgreementService legalEntityAgreementService)
    {
        // This does not assess whether an agreement has been signed or not
        When(x => !string.IsNullOrWhiteSpace(x.AccountLegalEntityPublicHashedId), () =>
        {
            RuleFor(x => x.AccountLegalEntityPublicHashedId).CustomAsync(async (value, context, _) =>
            {
                var legalEntity = await legalEntityAgreementService.GetLegalEntityAsync(context.InstanceToValidate.EmployerAccountId, value);
                if (legalEntity is null)
                {
                    context.AddFailure(FieldIdentifiers.OrganisationName, "Enter a valid employer name");
                }
            });
        });
    }
}