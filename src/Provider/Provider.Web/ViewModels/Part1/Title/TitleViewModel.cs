using System;
using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using Microsoft.AspNetCore.Mvc;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.TitleValidationMessages;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Title
{
    public class TitleViewModel
    {
        public Guid? VacancyId { get; set; }
        public string EmployerAccountId { get; set; }
        public long Ukprn { get; set; }
        public string Title { get; set; }

        [TypeOfInteger(ErrorMessage = ErrMsg.TypeOfInteger.NumberOfPositions)]
        public string NumberOfPositions { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Title),
            nameof(NumberOfPositions)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }
        public string FormPostRouteName => VacancyId.HasValue ? RouteNames.Title_Post : RouteNames.CreateVacancy_Post;
        
        //public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

    }
}