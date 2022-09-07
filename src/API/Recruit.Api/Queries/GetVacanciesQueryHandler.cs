using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Api.Helpers;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetVacanciesQueryHandler : IRequestHandler<GetVacanciesQuery, GetVacanciesResponse>
    {
        private readonly ILogger<GetVacanciesQueryHandler> _logger;
        private readonly IVacancySummaryMapper _mapper;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IEmployerVacancyClient _employerVacancyClient;

        public GetVacanciesQueryHandler(ILogger<GetVacanciesQueryHandler> logger, IVacancySummaryMapper mapper, IProviderVacancyClient providerVacancyClient, IEmployerVacancyClient employerVacancyClient)
        {
            _logger = logger;
            _mapper = mapper;
            _providerVacancyClient = providerVacancyClient;
            _employerVacancyClient = employerVacancyClient;
        }

        public async Task<GetVacanciesResponse> Handle(GetVacanciesQuery request, CancellationToken cancellationToken)
        {
            var validationErrors = ValidateRequest(request);

            if (validationErrors.Any())
            {
                return new GetVacanciesResponse { ResultCode = ResponseCode.InvalidRequest, ValidationErrors = validationErrors.Cast<object>().ToList() };
            }

            IList<Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary> vacancies;
            var isRequestingProviderOwnedVacancies = request.Ukprn.HasValue;
            long totalVacancies;
            if (isRequestingProviderOwnedVacancies)
            {
                var providerVacanciesTask = _providerVacancyClient.GetDashboardAsync(request.Ukprn.Value,
                    VacancyType.Apprenticeship, request.PageNo, FilteringOptions.All, null);
                var totalVacanciesTask = _providerVacancyClient.GetVacancyCount(request.Ukprn.Value,
                    VacancyType.Apprenticeship, FilteringOptions.All, null);

                await Task.WhenAll(providerVacanciesTask, totalVacanciesTask);

                vacancies = providerVacanciesTask.Result?.Vacancies?.ToList() ?? new List<Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary>();
                totalVacancies = totalVacanciesTask.Result;
            }
            else
            {
                var employerVacanciesTask = _employerVacancyClient.GetDashboardAsync(request.EmployerAccountId,
                    request.PageNo, FilteringOptions.All, null);
                var totalVacanciesTask = _employerVacancyClient.GetVacancyCount(request.EmployerAccountId, VacancyType.Apprenticeship, FilteringOptions.All, null);

                await Task.WhenAll(employerVacanciesTask, totalVacanciesTask);
                
                vacancies = employerVacanciesTask.Result?.Vacancies?.ToList() ?? new List<Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary>();
                totalVacancies = totalVacanciesTask.Result;
            }

            if (vacancies.Any() == false)
            {
                return new GetVacanciesResponse { ResultCode = ResponseCode.NotFound };
            }
            
            var pageNo = PagingHelper.GetRequestedPageNo(request.PageNo, 25, Convert.ToInt32(totalVacancies));
            var totalPages = PagingHelper.GetTotalNoOfPages(25, Convert.ToInt32(totalVacancies));

            var responseVacancies = GetVacanciesForPage(vacancies, isRequestingProviderOwnedVacancies);

            return new GetVacanciesResponse
            {
                ResultCode = ResponseCode.Success,
                Data = new VacanciesSummary(responseVacancies, request.PageSize, pageNo, Convert.ToInt32(totalVacancies), totalPages)
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

        private List<VacancySummary> GetVacanciesForPage(IList<Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary> vacancies, bool isRequestingProviderOwnedVacancies)
        {
            var responseVacancies = vacancies
                .Select(vs => _mapper.MapFromVacancySummaryProjection(vs, isRequestingProviderOwnedVacancies))
                .ToList();

            return responseVacancies;
        }
    }
}