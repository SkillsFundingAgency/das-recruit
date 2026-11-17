using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetRouteResponse
{
    public IEnumerable<GetRouteResponseItem> Routes { get; set; }

    public class GetRouteResponseItem
    {
        public string Route { get; set; }
        public int Id { get; set; }
    }
}