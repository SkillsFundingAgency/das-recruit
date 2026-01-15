using System.Collections.Generic;
using MediatR;
using SFA.DAS.Recruit.Api.Models;
using System.Threading.Tasks;
using System.Threading;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetClosedVacanciesByReferenceQuery(List<long> vacancyReferences) : IRequest<GetClosedVacanciesByReferenceQueryResponse>
    {
        public List<long> VacancyReferences { get; set; } = vacancyReferences;
    }

    public class GetClosedVacanciesByReferenceQueryResponse : ResponseBase;

    public class GetClosedVacanciesByReferenceQueryHandler(IVacancyQuery vacancyQuery)
        : IRequestHandler<GetClosedVacanciesByReferenceQuery, GetClosedVacanciesByReferenceQueryResponse>
    {
        public async Task<GetClosedVacanciesByReferenceQueryResponse> Handle(GetClosedVacanciesByReferenceQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await vacancyQuery.FindClosedVacancies(request.VacancyReferences);

            return new GetClosedVacanciesByReferenceQueryResponse
            {
                Data = new ClosedVacanciesSummary
                {
                    Vacancies = queryResult
                },
                ResultCode = queryResult == null ? ResponseCode.NotFound : ResponseCode.Success
            };
        }
    }
}
