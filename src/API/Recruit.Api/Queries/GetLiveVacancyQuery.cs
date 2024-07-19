using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
                return new GetLiveVacancyQueryResponse
                {
                    Data = null,
                    ResultCode = ResponseCode.NotFound
                };
            }

            if (queryResult.Wage == null)
            {
                _logger.LogError($"Wage for vacancy {request.VacancyReference} is null");
                return new GetLiveVacancyQueryResponse
                {
                    Data = null,
                    ResultCode = ResponseCode.NotFound
                };
            }

            _logger.LogInformation($"Adding wage data to vacancy {request.VacancyReference}");
            var wageObject = JsonConvert.SerializeObject(queryResult.Wage);
            _logger.LogInformation($"Wage object: {wageObject}");

            try
            {
                queryResult.AddWageData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding wage data to vacancy {request.VacancyReference}");
            }

            return new GetLiveVacancyQueryResponse
            {
                Data = queryResult,
                ResultCode = queryResult == null ? ResponseCode.NotFound : ResponseCode.Success
            };
        }
    }
}
