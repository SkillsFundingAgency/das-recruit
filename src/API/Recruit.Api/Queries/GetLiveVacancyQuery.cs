using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Recruit.Api.Extensions;
using SFA.DAS.Recruit.Api.Models;
using IQueryStoreReader = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.IQueryStoreReader;


namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetLiveVacancyQuery : IRequest<GetLiveVacancyQueryResponse>
    {
        public GetLiveVacancyQuery(long vacancyReference)
        {
            VacancyReference = vacancyReference;
        }

        public long VacancyReference { get; set; }
    }

    public class GetLiveVacancyQueryResponse : ResponseBase
    {
    }

    public class GetLiveVacancyQueryHandler : IRequestHandler<GetLiveVacancyQuery, GetLiveVacancyQueryResponse>
    {
        private readonly IQueryStoreReader _queryStoreReader;

        public GetLiveVacancyQueryHandler(IQueryStoreReader queryStoreReader)
        {
            _queryStoreReader = queryStoreReader;
        }

        public async Task<GetLiveVacancyQueryResponse> Handle(GetLiveVacancyQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await _queryStoreReader.GetLiveVacancy(request.VacancyReference);

            queryResult.AddWageData();

            return new GetLiveVacancyQueryResponse
            {
                Data = queryResult,
                ResultCode = queryResult == null ? ResponseCode.NotFound : ResponseCode.Success
            };
        }
    }
}
