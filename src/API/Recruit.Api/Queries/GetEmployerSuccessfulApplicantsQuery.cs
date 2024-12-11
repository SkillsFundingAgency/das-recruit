using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using SFA.DAS.Recruit.Api.Helpers;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Services;
using ApplicationReviewStatus = SFA.DAS.Recruit.Api.Services.ApplicationReviewStatus;

namespace SFA.DAS.Recruit.Api.Queries;

public record GetEmployerSuccessfulApplicantsQuery(string EmployerAccountId) : IRequest<GetEmployerSuccessfulApplicantsQueryResponse>;

public class GetEmployerSuccessfulApplicantsQueryResponse : ResponseBase;

public class GetEmployerSuccessfulApplicantsQueryHandler(
    IVacancyQuery vacancyQuery,
    IQueryStoreReader queryStoreReader)
    : IRequestHandler<GetEmployerSuccessfulApplicantsQuery, GetEmployerSuccessfulApplicantsQueryResponse>
{
    private const int MaxDegreeOfParallelism = 10;

    public async Task<GetEmployerSuccessfulApplicantsQueryResponse> Handle(GetEmployerSuccessfulApplicantsQuery request, CancellationToken cancellationToken)
    {
        var validationErrors = ValidateRequest(request);

        if (validationErrors.Count != 0)
        {
            return new GetEmployerSuccessfulApplicantsQueryResponse
            {
                ResultCode = ResponseCode.InvalidRequest,
                ValidationErrors = validationErrors.Cast<object>().ToList()
            };
        }

        var vacancies = await vacancyQuery.GetVacanciesByEmployerAccountAsync<VacancyIdentifier>(request.EmployerAccountId);

        var successfulApplications = await GetSuccessfulApplications(vacancies);

        return new GetEmployerSuccessfulApplicantsQueryResponse
        {
            ResultCode = ResponseCode.Success,
            Data = successfulApplications.Select(ApplicantSummaryMapper.MapFromVacancyApplication)
                .OrderBy(x=> x.LastName)
                .ThenBy(x=> x.FirstName)
        };
    }

    private async Task<ConcurrentBag<VacancyApplication>> GetSuccessfulApplications(IEnumerable<VacancyIdentifier> vacancies)
    {
        var successfulApplications = new ConcurrentBag<VacancyApplication>();
        var options = new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism };

        await Parallel.ForEachAsync(vacancies, options, async (vacancy, _) =>
        {
            var response = await queryStoreReader.GetVacancyApplicationsAsync(vacancy.VacancyReference.ToString());

            if (response != null && response.Applications.Count > 0)
            {
                var filteredApplications = FilterSuccessfulApplicants(response);

                foreach (var application in filteredApplications)
                {
                    successfulApplications.Add(application);
                }
            }
        });

        return successfulApplications;
    }

    private static List<VacancyApplication> FilterSuccessfulApplicants(VacancyApplications vacancyApplications)
    {
        return vacancyApplications.Applications
            .Where(app => app.Status.Equals(ApplicationReviewStatus.Successful))
            .ToList();
    }

    private static List<string> ValidateRequest(GetEmployerSuccessfulApplicantsQuery request)
    {
        const string employerAccountIdFieldName = nameof(request.EmployerAccountId);
        const string employerAccountIdRegex = "^[A-Z0-9]{6}$";
        var validationErrors = new List<string>();

        if (string.IsNullOrEmpty(request.EmployerAccountId) || !Regex.IsMatch(request.EmployerAccountId, employerAccountIdRegex))
        {
            validationErrors.Add($"Invalid {FieldNameHelper.ToCamelCasePropertyName(employerAccountIdFieldName)}");
        }

        return validationErrors;
    }
}