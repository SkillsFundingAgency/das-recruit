using System;
using Microsoft.AspNetCore.Mvc;


namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Title
{
    public class TitleEditModel
    {
        [FromRoute]
        public string EmployerAccountId { get; set; }
        [FromRoute]
        public Guid? VacancyId { get; set; }
        public string Title { get; set; }
        public string ReferredUkprn { get; set; }
        public string ReferredProgrammeId { get; set; }
        public bool ReferredFromSavedFavourites { get; set; }
        public bool IsReferredFromSavedFavourites => ReferredFromSavedFavourites && (!string.IsNullOrWhiteSpace(ReferredProgrammeId) || !string.IsNullOrWhiteSpace(ReferredUkprn));
    }
}
