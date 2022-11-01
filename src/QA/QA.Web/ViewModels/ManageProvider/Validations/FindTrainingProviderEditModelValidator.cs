using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using FluentValidation;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.QA.Web.ViewModels.ManageProvider.Validations
{
    public class FindTrainingProviderEditModelValidator : AbstractValidator<FindTrainingProviderEditModel>
    {
        public FindTrainingProviderEditModelValidator(ITrainingProviderService service)
        {
            RuleFor(m => m.Ukprn)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("Please add a UKPRN to continue")
                .ValidUkprn()
                .WithMessage("Please add a valid UKPRN to continue");

            RuleFor(m => m.Postcode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Please add a postcode to continue")
                .ValidPostCode()
                .WithMessage("Please enter a valid postcode");

            RuleFor(vm => vm)
                .MustAsync((m, model, cancellation) => DoesProviderExists(model, service))
                .When(c => c.Ukprn != null && ValidationConstants.UkprnRegex.IsMatch(c.Ukprn) && !string.IsNullOrWhiteSpace(c.Postcode) && ValidationConstants.PostcodeRegex.IsMatch(c.Postcode))
                .WithMessage("The postcode provided doesn't match the training provider");
        }

        private async Task<bool> DoesProviderExists(FindTrainingProviderEditModel model, ITrainingProviderService service)
        {
            long.TryParse(model.Ukprn, out var ukprn);
            var provider = await service.GetProviderAsync(ukprn);
            return provider != null && provider.Address.Postcode.IsEqualWithoutSymbols(model.Postcode);
        }
    }
}