using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Vacancies.Client.Application.Services.Models;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class IApprenticeshipProgrammeExensions
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
                    Name = $"{p.Title}, Level: {p.Level} ({p.ApprenticeshipType.ToString()})"
                }).OrderBy(p => p.Name);
        }
    }
}
