using System;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Microsoft.AspNetCore.Mvc;
using ErrMsg = Esfa.Recruit.Employer.Web.ViewModels.ValidationMessages.TitleValidationMessages;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Title
{
    public class TitleEditModel
    {
        [FromRoute]
        public string EmployerAccountId { get; set; }

        [FromRoute]
        public Guid? VacancyId { get; set; }

        public string Title { get; set; }

        [TypeOfInteger(ErrorMessage = ErrMsg.TypeOfInteger.NumberOfPositions)]
        public string NumberOfPositions { get; set; }
    }
}
