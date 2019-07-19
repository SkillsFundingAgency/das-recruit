﻿using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using FluentValidation;

namespace Esfa.Recruit.Qa.Web.ViewModels.Reports.ApplicationsReport
{
    public class ApplicationsReportCreateEditModelValidator : AbstractValidator<ApplicationsReportCreateEditModel>
    {
        public ApplicationsReportCreateEditModelValidator(ITimeProvider timeProvider)
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
                              .Cascade(CascadeMode.StopOnFirstFailure)
                              .Must(date => date.AsDateTimeUk() < timeProvider.NextDay)
                              .WithMessage("Date to cannot be in the future");

                          RuleFor(x => x.FromDate)
                              .Must((model, _) => model.FromDate.AsDateTimeUk() < model.ToDate.AsDateTimeUk())
                              .WithMessage("Date from must be less than Date to");
                      });
        }
    }
}
