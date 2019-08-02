using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using FluentValidation;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.QA.Web.ViewModels.ManageProvider.Validations
{
    public class FindTrainingProviderEditModelValidator : AbstractValidator<FindTrainingProviderEditModel>
    {
        public FindTrainingProviderEditModelValidator(ITrainingProviderService service)
        {
            RuleFor(m => m.Ukprn)
                .NotNull()
                .WithMessage("Please add a UKPRN to continue")
                .GreaterThan(10000000)
                .WithMessage("Please add a valid UKPRN to continue");

            RuleFor(m => m.Postcode)
                .NotEmpty()
                .WithMessage("Please add a postcode to continue");

            When(x => x.Ukprn != null && string.IsNullOrWhiteSpace(x.Postcode) == false, () => 
            {
                RuleFor(m => m.Ukprn)
                    .MustAsync((model, ukprn, cancellation) => DoesProviderExists(model, service))
                    .WithMessage("Please add a valid postcode and UKPRN to continue");
            });
        }

        private async Task<bool> DoesProviderExists(FindTrainingProviderEditModel model, ITrainingProviderService service)
        {
            if(model.Ukprn == null) return true;
            var provider = await service.GetProviderAsync(model.Ukprn.GetValueOrDefault());
            return provider != null && provider.Address.Postcode.IsEqualWithoutSymbols(model.Postcode);
        }
    }
}