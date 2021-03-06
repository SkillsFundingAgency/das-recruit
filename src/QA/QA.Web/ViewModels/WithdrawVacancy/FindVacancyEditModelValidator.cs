﻿using FluentValidation;

namespace Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy
{
    public class FindVacancyEditModelValidator : AbstractValidator<FindVacancyEditModel>
    {
        public FindVacancyEditModelValidator()
        {
            RuleFor(x => x.VacancyReference)
                .NotEmpty()
                .WithMessage("Please enter a vacancy reference number to continue");
        }
    }
}
