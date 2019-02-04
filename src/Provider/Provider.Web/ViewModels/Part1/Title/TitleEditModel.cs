using System;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.TitleValidationMessages;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Title
{
    public class TitleEditModel
    {
        [FromRoute]
        public Guid? VacancyId { get; set; }

        public string Title { get; set; }

        [TypeOfInteger(ErrorMessage = ErrMsg.TypeOfInteger.NumberOfPositions)]
        public string NumberOfPositions { get; set; }

        public string EmployerAccountId { get; set; }
    }
}
