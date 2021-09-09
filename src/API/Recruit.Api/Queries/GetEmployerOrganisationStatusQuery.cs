using MediatR;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetEmployerOrganisationStatusQuery : IRequest<GetOrganisationStatusResponse>
    {
        public string EmployerAccountId { get; }

        public GetEmployerOrganisationStatusQuery(string employerAccountId)
        {
            EmployerAccountId = employerAccountId;
        }
    }
}