using MediatR;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetEmployerSummaryQuery : IRequest<GetEmployerSummaryResponse>
    {
        public string EmployerAccountId { get; }

        public GetEmployerSummaryQuery(string employerAccountId)
        {
            EmployerAccountId = employerAccountId;
        }
    }
}