using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using ErrMsg = Esfa.Recruit.Employer.Web.ViewModels.ValidationMessages.WageValidationMessages;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage
{
    public class WageEditModel
    {
        [FromRoute]
        [Required]
        public string EmployerAccountId { get; set; }

        [FromRoute]
        [ValidGuid]
        public Guid VacancyId { get; set; }

        [Required(ErrorMessage = ErrMsg.Required.Duration )]
        [TypeOfInteger(ErrorMessage = ErrMsg.TypeOfInteger.Duration)]
        public string Duration { get; set; }

        public DurationUnit DurationUnit { get; set; }

        [Required(ErrorMessage = ErrMsg.Required.WorkingWeekDescription)]
        [FreeText(ErrorMessage = ErrMsg.FreeText.WorkingWeekDescription)]
        [StringLength(250, ErrorMessage = ErrMsg.StringLength.WorkingWeekDescription )]
        public string WorkingWeekDescription { get; set; }

        [Required(ErrorMessage = ErrMsg.Required.WeeklyHours)]
        [TypeOfDecimal(2, ErrorMessage = ErrMsg.TypeOfDecimal.WeeklyHours)]
        public string WeeklyHours { get; set; }

        public WageType WageType { get; set; }

        [TypeOfMoney(ErrorMessage = ErrMsg.TypeOfMoney.FixedWageYearlyAmount)]
        public string FixedWageYearlyAmount { get; set; }

        [StringLength(240, ErrorMessage = ErrMsg.StringLength.WageAdditionalInformation)]
        [FreeText(ErrorMessage = ErrMsg.FreeText.WageAdditionalInformation)]
        public string WageAdditionalInformation { get; set; }
    }
}
