using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class ApprenticeshipProgrammesExensions
    {
        public static IEnumerable<ApprenticeshipProgrammeViewModel> ToViewModel(
            this ApprenticeshipProgrammes programmes)
        {
            if (programmes?.Programmes == null)
            {
                return Enumerable.Empty<ApprenticeshipProgrammeViewModel>();
            }

            return programmes.Programmes.Select(p =>
                new ApprenticeshipProgrammeViewModel
                {
                    Id = p.Id,
                    Name = $"{p.Title}, Level: {p.Level} ({p.ApprenticeshipType.ToString()})"
                }).OrderBy(p => p.Name);
        }
    }
}
