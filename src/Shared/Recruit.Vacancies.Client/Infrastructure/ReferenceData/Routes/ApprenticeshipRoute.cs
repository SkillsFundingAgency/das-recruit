﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipRoute
{
    public class ApprenticeshipRoute : IApprenticeshipRoute
    {
        public string Route { get; set; }
        public int Id { get; set; }

        public static implicit operator ApprenticeshipRoute(GetRouteResponse.GetRouteResponseItem source)
        {
            return new ApprenticeshipRoute
            {
                Id = source.Id,
                Route = source.Route,
            };
        }
    }
}