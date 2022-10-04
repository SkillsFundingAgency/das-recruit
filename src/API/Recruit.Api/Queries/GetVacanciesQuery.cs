using MediatR;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetVacanciesQuery : IRequest<GetVacanciesResponse>
    {
        public string EmployerAccountId { get; }
        public long? Ukprn { get; }
        public int PageSize { get; }
        public int PageNo { get; }

        public GetVacanciesQuery(string employerAccountId, long? ukprn, int pageSize, int pageNo)
        {
            EmployerAccountId = employerAccountId;
            Ukprn = ukprn;
            PageSize = pageSize;
            PageNo = pageNo;
        }
    }
}