using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public class TrainingProgrammeValidator : AbstractValidator<Programme> 
    {
        public TrainingProgrammeValidator(long ruleId)
        {
            var vacancyRules = (VacancyRuleSet)ruleId;

            RuleFor(x => x.Id)
                .NotEmpty()
                    .WithMessage("Select  apprenticeship training")
                    .WithErrorCode("25")
                .WithRuleId(vacancyRules);

            RuleFor(x => x.Level)
                .NotEmpty()
                    .WithMessage("Select  apprenticeship training")
                    .WithErrorCode("25")
                .WithRuleId(vacancyRules);

            RuleFor(x => x.LevelName)
                .NotEmpty()
                    .WithMessage("Select  apprenticeship training")
                    .WithErrorCode("25")
                .WithRuleId(vacancyRules);

            RuleFor(x => x.Title)
                .NotEmpty()
                    .WithMessage("Select  apprenticeship training")
                    .WithErrorCode("25")
                .WithRuleId(vacancyRules);

            RuleFor(x => x.TrainingType)
                .NotEmpty()
                    .WithMessage("Select  apprenticeship training")
                    .WithErrorCode("25")
                .IsInEnum()
                    .WithMessage("Select  apprenticeship training")
                    .WithErrorCode("25")
                .WithRuleId(vacancyRules);
		}
	}
}
