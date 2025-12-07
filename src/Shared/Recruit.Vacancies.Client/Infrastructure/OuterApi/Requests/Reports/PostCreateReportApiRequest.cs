using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using static Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Reports.PostCreateReportApiRequest;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Reports;
public record PostCreateReportApiRequest(PostCreateReportApiRequestData Payload) : IPostApiRequest
{
    public string PostUrl => "reports/create";
    public object Data { get; set; } = Payload;

    public record PostCreateReportApiRequestData
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required string UserId { get; init; }
        public required string CreatedBy { get; init; }
        public required DateTime FromDate { get; init; }
        public required DateTime ToDate { get; init; }
        public long? Ukprn { get; init; }
        public required string OwnerType { get; init; }
    }
}