using System.Collections.Generic;
using MediatR;
using SFA.DAS.Recruit.Api.Models;
using System.Threading.Tasks;
using System.Threading;
using IQueryStoreReader = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.IQueryStoreReader;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetClosedVacanciesByReferenceQuery(List<long> vacancyReferences) : IRequest<GetClosedVacanciesByReferenceQueryResponse>
    {
        public List<long> VacancyReferences { get; set; } = vacancyReferences;
    }

    public class GetClosedVacanciesByReferenceQueryResponse : ResponseBase
    {
    }

    public class GetClosedVacanciesByReferenceQueryHandler(IQueryStoreReader queryStoreReader)
        : IRequestHandler<GetClosedVacanciesByReferenceQuery, GetClosedVacanciesByReferenceQueryResponse>
    {
        public async Task<GetClosedVacanciesByReferenceQueryResponse> Handle(GetClosedVacanciesByReferenceQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await queryStoreReader.GetClosedVacancies(request.VacancyReferences);

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
