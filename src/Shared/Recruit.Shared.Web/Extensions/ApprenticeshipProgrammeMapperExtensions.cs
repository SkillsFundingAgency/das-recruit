using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class ApprenticeshipProgrammeMapperExtensions
    {
        public static IEnumerable<ApprenticeshipProgrammeViewModel> ToViewModel(
            this IEnumerable<IApprenticeshipProgramme> programmes)
        {
            if (programmes == null)
            {
                return Enumerable.Empty<ApprenticeshipProgrammeViewModel>();
            }

            return programmes.Select(p =>
                new ApprenticeshipProgrammeViewModel
                {
                    Id = p.Id,
                    Name = $"{p.Title}, Level: {p.Level}, level {(int)p.Level} ({p.ApprenticeshipType.ToString()})"
                }).OrderBy(p => p.Name);
        }
    }
}
