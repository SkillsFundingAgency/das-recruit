using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class RouteModel
    {
        private string _employerAccountId;

        [FromRoute]
        public string EmployerAccountId
        {
            get => _employerAccountId;
            set => _employerAccountId = value.ToUpper();
        }
    }
}
