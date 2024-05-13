using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using SFA.DAS.Recruit.Api.Models;


namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetClosedVacancyQuery(long vacancyReference) : IRequest<GetClosedVacancyQueryResponse>
    {
        public long VacancyReference { get; set; } = vacancyReference;
    }

    public class GetClosedVacancyQueryResponse : ResponseBase
    {
    }

    public class GetClosedVacancyQueryHandler(IVacancyQuery queryStoreReader)
        : IRequestHandler<GetClosedVacancyQuery, GetClosedVacancyQueryResponse>
    {
        public async Task<GetClosedVacancyQueryResponse> Handle(GetClosedVacancyQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await queryStoreReader.GetVacancyAsync(request.VacancyReference);

            if (queryResult.Status != VacancyStatus.Closed)
            {
                queryResult = null;
            }
            
            return new GetClosedVacancyQueryResponse
            {
                Data = queryResult,
                ResultCode = queryResult == null ? ResponseCode.NotFound : ResponseCode.Success
            };
        }
    }
}
