using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public record GetAllApplicationsByIdApiRequest(GetAllApplicationsByIdApiRequestData Payload) : IPostApiRequest
    {
        public string PostUrl
        {
            get
            {
                return "applications/getAll";
            }
        }

        public object Data { get; set; } = Payload;
    }

    public class GetAllApplicationsByIdApiRequestData
    {
        public required List<Guid> ApplicationIds { get; init; } = [];
        public bool IncludeDetails { get; set; } = false;
    }
}