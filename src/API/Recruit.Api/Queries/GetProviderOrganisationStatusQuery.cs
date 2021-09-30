using MediatR;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetProviderOrganisationStatusQuery : IRequest<GetOrganisationStatusResponse>
    {
        public long Ukprn { get; }

        public GetProviderOrganisationStatusQuery(long ukprn)
        {
            Ukprn = ukprn;
        }
    }
}