﻿using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    internal class QualificationValidator : AbstractValidator<Qualification>
    {
        public QualificationValidator(long ruleId, QualificationsConfiguration qualificationsConfiguration)
        {
            var configuration = qualificationsConfiguration;

            RuleFor(x => x.QualificationType)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                    .WithMessage("Select a qualification")
                    .WithErrorCode("53")
                .Must(q => configuration.QualificationTypes.Contains(q))
                    .WithMessage("Invalid qualification type")
                    .WithErrorCode("57")
                .WithRuleId(ruleId);

            RuleFor(x => x.Subject)
                .NotEmpty()
                    .WithMessage("Select a qualification subject")
                    .WithErrorCode("54")
                .MaximumLength(50)
                    .WithMessage("The qualification must be  less than {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .WithRuleId(ruleId);

            RuleFor(x => x.Grade)
                .NotEmpty()
                    .WithMessage("Select a qualification grade")
                    .WithErrorCode("55")
                .MaximumLength(30)
                    .WithMessage("The grade must be less than {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .WithRuleId(ruleId);

            RuleFor(x => x.Weighting)
                .NotEmpty()
                    .WithMessage("Select if this is a desired or an essential qualification")
                    .WithErrorCode("56")
                .WithRuleId(ruleId);
        }
    }
}
