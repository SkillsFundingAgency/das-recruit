using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Recruit.Api.Models;
using IQueryStoreReader = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.IQueryStoreReader;


namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetClosedVacancyQuery(long vacancyReference) : IRequest<GetClosedVacancyQueryResponse>
    {
        public long VacancyReference { get; set; } = vacancyReference;
    }

    public class GetClosedVacancyQueryResponse : ResponseBase
    {
    }

    public class GetClosedVacancyQueryHandler(IQueryStoreReader queryStoreReader)
        : IRequestHandler<GetClosedVacancyQuery, GetClosedVacancyQueryResponse>
    {
        public async Task<GetClosedVacancyQueryResponse> Handle(GetClosedVacancyQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await queryStoreReader.GetClosedVacancy(request.VacancyReference);

            return new GetClosedVacancyQueryResponse
            {
                Data = queryResult,
                ResultCode = queryResult == null ? ResponseCode.NotFound : ResponseCode.Success
            };
        }
    }
}
