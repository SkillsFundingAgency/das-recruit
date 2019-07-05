﻿using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
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

            return programmes.Select(ToViewModel).OrderBy(p => p.Name);
        }

        public static ApprenticeshipProgrammeViewModel ToViewModel(this IApprenticeshipProgramme programme)
        {
            return new ApprenticeshipProgrammeViewModel
            {
                Id = programme.Id,
                Name = $"{programme.Title}, Level: {programme.Level}, level {(int)programme.Level} ({programme.ApprenticeshipType.ToString()})"
            };
        }
    }
}
