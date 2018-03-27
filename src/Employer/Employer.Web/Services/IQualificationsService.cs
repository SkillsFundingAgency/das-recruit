using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IQualificationsService
    {
        List<string> GetQualificationTypes();

        List<Qualification> SortQualifications(IEnumerable<Qualification> qualificationsToSort);
    }
}
