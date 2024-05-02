using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    /// <summary>
    /// Unconditionally validates a <see cref="Qualification"/>.
    /// <seealso cref="VacancyQualificationsValidator"/>
    /// </summary>
    internal class QualificationValidator : QualificationValidatorBase
    {
        public QualificationValidator(IQualificationsProvider qualificationsProvider, IProfanityListProvider profanityListProvider, IFeature feature)
            : base(qualificationsProvider,profanityListProvider, feature)
        {
        }
    }

    /// <summary>
    /// Registers validation with a specific rule ID.
    /// This is used by the <see cref="FluentVacancyValidator"/> to conditionally validate every
    /// <see cref="Qualification"/> in a vacancy.
    /// </summary>
    internal class VacancyQualificationsValidator : QualificationValidatorBase
    {
        public VacancyQualificationsValidator(long ruleId, IQualificationsProvider qualificationsProvider, IProfanityListProvider profanityListProvider, IFeature feature)
            : base(ruleId, qualificationsProvider,profanityListProvider,feature)
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
        private readonly IFeature _feature;
        private readonly IList<string> _qualificationTypes;

        public QualificationValidatorBase(IQualificationsProvider qualificationsProvider, IProfanityListProvider profanityListProvider, IFeature feature)
            : this(0, qualificationsProvider, profanityListProvider, feature)
        {
            _feature = feature;
        }

        public QualificationValidatorBase(long ruleId, IQualificationsProvider qualificationsProvider, IProfanityListProvider profanityListProvider, IFeature feature)
        {
            _qualificationTypes = qualificationsProvider.GetQualificationsAsync().Result ?? new List<string>();
            
            RuleFor(x => x.QualificationType)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage("Select a qualification")
                    .WithErrorCode("53")
                .WithState(_=>ruleId)
                .Must(_qualificationTypes.Contains)
                    .WithMessage("Invalid qualification type")
                    .WithErrorCode("57")
                .WithState(_=>ruleId)
                .WithState(_ => ruleId);

            RuleFor(x => x.Subject)
                .NotEmpty()
                    .WithMessage("Enter the subject")
                    .WithErrorCode("54")
                .WithState(_=>ruleId)
                .MaximumLength(50)
                    .WithMessage("The qualification must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .WithState(_=>ruleId)
                .ValidFreeTextCharacters()
                    .WithMessage("Subject contains some invalid characters")
                    .WithErrorCode("6")
                .WithState(_=>ruleId)
                .ProfanityCheck(profanityListProvider)
                .WithMessage("Subject must not contain a banned word or phrase.")
                .WithErrorCode("618")
                .WithState(_=>ruleId)
                .WithState(_ => ruleId);

            RuleFor(x => x.Grade)
                .NotEmpty()
                    .WithMessage("Enter the grade")
                    .WithErrorCode("55")
                .WithState(_=>ruleId)
                .MaximumLength(30)
                    .WithMessage("The grade should be no longer than {MaxLength} characters")
                    .WithErrorCode("7")
                .WithState(_=>ruleId)
                .ValidFreeTextCharacters()
                    .WithMessage("Grade contains some invalid characters")
                    .WithErrorCode("6")
                .WithState(_=>ruleId)
                .ProfanityCheck(profanityListProvider)
                .WithMessage("Grade must not contain a banned word or phrase.")
                .WithErrorCode("619")
                .WithState(_ => ruleId);

            When(x => x.QualificationType != null && x.QualificationType.Contains("GCSE"), () =>
            {
                RuleFor(x => x.Grade)
                    .Matches("[1-9]")
                        .WithMessage("GCSEs must include the 1-9 grading system")
                        .WithErrorCode("115")
                    .WithState(_ => ruleId);
                
                RuleFor(x=>x.Grade)
                    .MaximumLength(1)
                    .WithMessage("GCSEs must include the 1-9 grading system")
                    .WithErrorCode("115")
                    .WithState(_ => ruleId);
            });
            if (feature.IsFeatureEnabled("FaaV2Improvements"))
            {
                When(x => x.QualificationType != null && x.QualificationType.Contains("BTEC"), () =>
                {
                    RuleFor(x => x.Level)
                        .NotEmpty()
                        .WithMessage("Select your BTEC level")
                        .WithErrorCode("1115")
                        .WithState(_ => ruleId);
                });
                When(x => x.QualificationType != null && x.QualificationType.Contains("Other"), () =>
                {
                    RuleFor(x => x.OtherQualificationName)
                        .NotEmpty()
                        .WithMessage("Enter the name of the qualification")
                        .WithErrorCode("2115")
                        .WithState(_ => ruleId);
                });
            }
            

            RuleFor(x => x.Weighting)
                .NotEmpty()
                    .WithMessage("Select if this qualification is essential or desirable")
                    .WithErrorCode("56")
                .WithState(_ => ruleId);
        }

    }
}
