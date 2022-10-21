using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using FluentValidation;

namespace Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport
{
    public class ProviderApplicationsReportCreateEditModelValidator : AbstractValidator<ProviderApplicationsReportCreateEditModel>
    {
        public ProviderApplicationsReportCreateEditModelValidator(ITimeProvider timeProvider)
        {
            RuleFor(x => x.DateRange)
                .NotNull()
                .WithMessage("You must select the time period for the report");

            When(x => x.DateRange == DateRangeType.Custom, () =>
            {
                RuleFor(x => x.FromDate)
                    .Must(date => date.AsDateTimeUk() != null)
                    .WithMessage("Date from format should be dd/mm/yyyy");

                RuleFor(x => x.ToDate)
                    .Must(date => date.AsDateTimeUk() != null)
                    .WithMessage("Date to format should be dd/mm/yyyy");
            });

            When(x => x.DateRange == DateRangeType.Custom &&
                      x.FromDate.AsDateTimeUk() != null &&
                      x.ToDate.AsDateTimeUk() != null, () =>
            {
                RuleFor(x => x.ToDate)
                    .Cascade(CascadeMode.Stop)
                    .Must(date => date.AsDateTimeUk() < timeProvider.NextDay)
                    .WithMessage("Date to cannot be in the future")
                    .Must((model, x) => model.ToDate.AsDateTimeUk() < model.FromDate.AsDateTimeUk().Value.AddMonths(3))
                    .WithMessage("Enter a date within three months from the start date");

                RuleFor(x => x.FromDate)
                    .Must((model, _) => model.FromDate.AsDateTimeUk() < model.ToDate.AsDateTimeUk())
                    .WithMessage("Date from must be earlier than Date to");
            });
        }
    }
}
