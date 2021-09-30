using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Api.Helpers;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Services;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetVacanciesQueryHandler : IRequestHandler<GetVacanciesQuery, GetVacanciesResponse>
    {
        private readonly ILogger<GetVacanciesQueryHandler> _logger;
        private readonly IQueryStoreReader _queryStoreReader;
        private readonly IVacancySummaryMapper _mapper;

        public GetVacanciesQueryHandler(ILogger<GetVacanciesQueryHandler> logger, IQueryStoreReader queryStoreReader, IVacancySummaryMapper mapper)
        {
            _logger = logger;
            _queryStoreReader = queryStoreReader;
            _mapper = mapper;
        }

        public async Task<GetVacanciesResponse> Handle(GetVacanciesQuery request, CancellationToken cancellationToken)
        {
            var validationErrors = ValidateRequest(request);

            if (validationErrors.Any())
            {
                return new GetVacanciesResponse { ResultCode = ResponseCode.InvalidRequest, ValidationErrors = validationErrors };
            }

            IList<VacancySummaryProjection> vacancies;
            var isRequestingProviderOwnedVacancies = request.Ukprn.HasValue;

            if (isRequestingProviderOwnedVacancies)
            {
                var dashboard = await _queryStoreReader.GetProviderDashboardAsync(request.Ukprn.Value);
                vacancies = dashboard?
                            .Vacancies
                            .Where(vs => vs.EmployerAccountId.Equals(request.EmployerAccountId))
                            .ToList();
            }
            else
            {
                var dashboard = await _queryStoreReader.GetEmployerDashboardAsync(request.EmployerAccountId);
                vacancies = dashboard?.Vacancies.ToList();
            }

            if (vacancies == null || vacancies.Any() == false)
            {
                return new GetVacanciesResponse { ResultCode = ResponseCode.NotFound };
            }

            if (request.LegalEntityId.HasValue)
            {
                vacancies = vacancies.Where(vs => vs.LegalEntityId == request.LegalEntityId.Value).ToList();

                if (vacancies.Any() == false)
                {
                    return new GetVacanciesResponse { ResultCode = ResponseCode.NotFound };
                }
            }

            var totalVacancies = vacancies.Count;
            var pageNo = PagingHelper.GetRequestedPageNo(request.PageNo, request.PageSize, totalVacancies);
            var totalPages = PagingHelper.GetTotalNoOfPages(request.PageSize, totalVacancies);

            var responseVacancies = GetVacanciesForPage(vacancies, pageNo, request.PageSize, isRequestingProviderOwnedVacancies);

            return new GetVacanciesResponse
            {
                ResultCode = ResponseCode.Success,
                Data = new VacanciesSummary(responseVacancies, request.PageSize, pageNo, totalVacancies, totalPages)
            };
        }

        private List<string> ValidateRequest(GetVacanciesQuery request)
        {
            const string employerAccountIdFieldName = nameof(request.EmployerAccountId);
            const string ukprnFieldName = nameof(request.Ukprn);
            const string employerAccountIdRegex = @"^[A-Z0-9]{6}$";
            const string ukprnRegex = @"^\d{8}$";
            var validationErrors = new List<string>();

            if (string.IsNullOrEmpty(request.EmployerAccountId) || Regex.IsMatch(request.EmployerAccountId, employerAccountIdRegex) == false)
            {
                validationErrors.Add($"Invalid {FieldNameHelper.ToCamelCasePropertyName(employerAccountIdFieldName)}");
            }

            if (request.Ukprn.HasValue && Regex.IsMatch(request.Ukprn.ToString(), ukprnRegex) == false)
            {
                validationErrors.Add($"Invalid {FieldNameHelper.ToCamelCasePropertyName(ukprnFieldName)}");
            }

            return validationErrors;
        }

        private List<VacancySummary> GetVacanciesForPage(IList<VacancySummaryProjection> vacancies, int pageNo, int pageSize, bool isRequestingProviderOwnedVacancies)
        {
            var skip = (pageNo - 1) * pageSize;

            var responseVacancies = vacancies
                .Skip(skip)
                .Take(pageSize)
                .Select(vs => _mapper.MapFromVacancySummaryProjection(vs, isRequestingProviderOwnedVacancies))
                .ToList();

            return responseVacancies;
        }
    }
}