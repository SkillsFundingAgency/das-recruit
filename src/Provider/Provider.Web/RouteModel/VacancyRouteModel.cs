using System;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class VacancyRouteModel
    {
        private string _ukprn;

        [FromRoute]
        public string Ukprn
        {
            get => _ukprn;
            set => _ukprn = value.ToUpper();
        }

        [FromRoute]
        public Guid VacancyId { get; set; }
    }
}