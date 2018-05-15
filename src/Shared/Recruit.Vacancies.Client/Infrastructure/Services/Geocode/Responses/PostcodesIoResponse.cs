using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Responses
{
    public class PostcodesIoResponse
    {
        public List<PostcodesIoResponseResult> Result { get; set; }
    }

    public class PostcodesIoResponseResult
    {
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
    }
}
