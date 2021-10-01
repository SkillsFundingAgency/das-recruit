using MediatR;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetVacanciesQuery : IRequest<GetVacanciesResponse>
    {
        public string EmployerAccountId { get; }
        public int? LegalEntityId { get; }
        public long? Ukprn { get; }
        public int PageSize { get; }
        public int PageNo { get; }

        public GetVacanciesQuery(string employerAccountId, int? legalEntityId, long? ukprn, int pageSize, int pageNo)
        {
            EmployerAccountId = employerAccountId;
            LegalEntityId = legalEntityId;
            Ukprn = ukprn;
            PageSize = pageSize;
            PageNo = pageNo;
        }
    }
}