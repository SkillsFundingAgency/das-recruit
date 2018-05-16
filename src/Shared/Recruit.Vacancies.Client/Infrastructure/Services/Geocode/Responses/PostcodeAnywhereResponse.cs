using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Responses
{
    public class PostcodeAnywhereResponse
    {
        public List<PostcodeAnywhereResponseItem> Items { get; set; }
    }

    public class PostcodeAnywhereResponseItem
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
