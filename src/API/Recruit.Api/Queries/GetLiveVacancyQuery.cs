using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<GetLiveVacancyQueryHandler> _logger;

        public GetLiveVacancyQueryHandler(IQueryStoreReader queryStoreReader, ILogger<GetLiveVacancyQueryHandler> logger)
        {
            _queryStoreReader = queryStoreReader;
            _logger = logger;
        }

        public async Task<GetLiveVacancyQueryResponse> Handle(GetLiveVacancyQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting live {request.VacancyReference}");

            var queryResult = await _queryStoreReader.GetLiveVacancy(request.VacancyReference);

            if (queryResult == null)
            {
                _logger.LogError($"Wage for vacancy {request.VacancyReference} is null");
                throw new InvalidOperationException($"Wage for vacancy {request.VacancyReference} is null");
            }

            if (queryResult.Wage == null)
            {
                _logger.LogError($"Wage for vacancy {request.VacancyReference} is null");
                throw new InvalidOperationException($"Wage for vacancy {request.VacancyReference} is null");
            }

            _logger.LogInformation($"Adding wage data to vacancy {request.VacancyReference}");
            _logger.LogInformation($"Wage type for vacancy {request.VacancyReference} is  {queryResult.Wage.WageType}");

            queryResult.AddWageData();

            return new GetLiveVacancyQueryResponse
            {
                Data = queryResult,
                ResultCode = queryResult == null ? ResponseCode.NotFound : ResponseCode.Success
            };
        }
    }
}
