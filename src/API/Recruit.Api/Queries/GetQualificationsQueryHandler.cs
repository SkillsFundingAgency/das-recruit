using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetQualificationsQueryHandler : IRequestHandler<GetQualificationsQuery, GetQualificationsQueryResponse>
    {
        private readonly IRecruitVacancyClient _recruitVacancyClient;

        public GetQualificationsQueryHandler(IRecruitVacancyClient recruitVacancyClient)
        {
            _recruitVacancyClient = recruitVacancyClient;
        }
        public async Task<GetQualificationsQueryResponse> Handle(GetQualificationsQuery request, CancellationToken cancellationToken)
        {
            var candidateSkills = await _recruitVacancyClient.GetCandidateQualificationsAsync();

            return new GetQualificationsQueryResponse
            {
                Data = candidateSkills,
                ResultCode = ResponseCode.Success
            };
        }
    }
}
