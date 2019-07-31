﻿using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    /// <summary>
    /// Registers validation with a specific rule ID.
    /// This is used by the <see cref="FluentVacancyValidator"/> to conditionally validate every
    /// <see cref="Qualification"/> in a vacancy.
    /// </summary>
    internal class VacancyQualificationsValidator : QualificationValidatorBase
    {
        public VacancyQualificationsValidator(long ruleId, IQualificationsProvider qualificationsProvider)
            : base(ruleId, qualificationsProvider)
        {
        }
    }

    /// <summary>
    /// Unconditionally validates a <see cref="Qualification"/>.
    /// <seealso cref="VacancyQualificationsValidator"/>
    /// </summary>
    internal class QualificationValidator : QualificationValidatorBase
    {
        public QualificationValidator(IQualificationsProvider qualificationsProvider)
            : base(qualificationsProvider)
        {
        }
    }


    /// <summary>
    /// Validates a <see cref="Qualification"/>.
    /// Descendant classes should call the appropriate constructor to indicate whether
    /// or not a RuleId should be added to each rule.
    /// </summary>
    internal class QualificationValidatorBase : AbstractValidator<Qualification>
    {
        private readonly IList<string> _qualifications;

        public QualificationValidatorBase(IQualificationsProvider qualificationsProvider)
            : this(0, qualificationsProvider)
        {
        }

        public QualificationValidatorBase(long ruleId, IQualificationsProvider qualificationsProvider)
        {
            _qualifications = qualificationsProvider.GetQualificationsAsync().Result ?? new List<string>();
            
            RuleFor(x => x.QualificationType)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                    .WithMessage("Select a qualification")
                    .WithErrorCode("53")
                .Must(_qualifications.Contains)
                    .WithMessage("Invalid qualification type")
                    .WithErrorCode("57")
                .WithRuleId(ruleId);

            RuleFor(x => x.Subject)
                .NotEmpty()
                    .WithMessage("Provide a subject")
                    .WithErrorCode("54")
                .MaximumLength(50)
                    .WithMessage("The qualification must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidFreeTextCharacters()
                    .WithMessage("Subject contains some invalid characters")
                    .WithErrorCode("6")
                .WithRuleId(ruleId);

            RuleFor(x => x.Grade)
                .NotEmpty()
                    .WithMessage("Provide a grade")
                    .WithErrorCode("55")
                .MaximumLength(30)
                    .WithMessage("The grade should be no longer than {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidFreeTextCharacters()
                    .WithMessage("Grade contains some invalid characters")
                    .WithErrorCode("6")
                .WithRuleId(ruleId);

            When(x => x.QualificationType != null && x.QualificationType.Contains("GCSE"), () =>
            {
                RuleFor(x => x.Grade)
                    .Matches("[1-9]")
                        .WithMessage("GCSEs must include the 1-9 grading system")
                        .WithErrorCode("115")
                    .WithRuleId(ruleId);
            });

            RuleFor(x => x.Weighting)
                .NotEmpty()
                    .WithMessage("Select if this is a desired or an essential qualification")
                    .WithErrorCode("56")
                .WithRuleId(ruleId);
        }

    }
}
