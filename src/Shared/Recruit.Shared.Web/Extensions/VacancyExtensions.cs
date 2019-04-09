using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class VacancyExtensions
    {
        public static IEnumerable<string> EmployerAddressForDisplay(this Vacancy vacancy)
        {
            if (vacancy.EmployerLocation == null)
                return Enumerable.Empty<string>();

            if (vacancy.IsAnonymous)
                return new[] { vacancy.EmployerLocation.PostcodeAsOutcode() };

            return new[]
                {
                    vacancy.EmployerLocation.AddressLine1,
                    vacancy.EmployerLocation.AddressLine2,
                    vacancy.EmployerLocation.AddressLine3,
                    vacancy.EmployerLocation.AddressLine4,
                    vacancy.EmployerLocation.Postcode
                }
                .Where(x => !string.IsNullOrEmpty(x));
        }
    }
}
