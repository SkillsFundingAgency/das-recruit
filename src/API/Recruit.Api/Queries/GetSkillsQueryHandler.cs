using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetSkillsQueryHandler : IRequestHandler<GetSkillsQuery, GetSkillsQueryResponse>
    {
        private readonly IRecruitVacancyClient _recruitVacancyClient;

        public GetSkillsQueryHandler (IRecruitVacancyClient recruitVacancyClient)
        {
            _recruitVacancyClient = recruitVacancyClient;
        }
        public async Task<GetSkillsQueryResponse> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
        {
            var candidateSkills = await _recruitVacancyClient.GetCandidateSkillsAsync();

            return new GetSkillsQueryResponse
            {
                Data = candidateSkills,
                ResultCode = ResponseCode.Success
            };
        }
    }
}