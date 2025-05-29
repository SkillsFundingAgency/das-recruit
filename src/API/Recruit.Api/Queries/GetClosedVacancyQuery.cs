using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
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
            try
            {
                var queryResult = await queryStoreReader.GetVacancyAsync(request.VacancyReference);
                if (queryResult.Status is not (VacancyStatus.Closed or VacancyStatus.Live))
                {
                    queryResult = null;
                }
            
                return new GetClosedVacancyQueryResponse
                {
                    Data = queryResult,
                    ResultCode = queryResult == null ? ResponseCode.NotFound : ResponseCode.Success
                };
            }
            catch (VacancyNotFoundException e)
            {
                return new GetClosedVacancyQueryResponse
                {
                    Data = null,
                    ResultCode = ResponseCode.NotFound
                };
            }
        }
    }
}
