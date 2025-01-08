using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using MediatR;
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Queries;

public record GetEmployerSuccessfulApplicantsQuery(string EmployerAccountId) : IRequest<GetEmployerSuccessfulApplicantsQueryResponse>
{
    public string EmployerAccountId { get; set; } = EmployerAccountId;
}

public class GetEmployerSuccessfulApplicantsQueryResponse : ResponseBase;

public class GetEmployerSuccessfulApplicantsQueryHandler(
    IVacancyQuery vacancyQuery,
    IRecruitVacancyClient vacancyClient)
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

        var successfulApplications = await GetSuccessfulApplications(vacancies.ToList());

        return new GetEmployerSuccessfulApplicantsQueryResponse
        {
            ResultCode = ResponseCode.Success,
            Data = successfulApplications?.OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
        };
    }

    private async Task<List<SuccessfulApplicant>> GetSuccessfulApplications(List<VacancyIdentifier> vacancies)
    {
        if (vacancies.Count == 0)
        {
            return null;
        }

        var successfulApplications = new ConcurrentBag<SuccessfulApplicant>();
        var options = new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism };

        await Parallel.ForEachAsync(vacancies, options, async (vacancy, _) =>
        {
            var response = await vacancyClient.GetVacancyApplicationsAsync(vacancy.VacancyReference.Value);

            if (response != null && response.Count > 0)
            {
                var filteredApplications = FilterSuccessfulApplicants(response);

                foreach (var application in filteredApplications)
                {
                    successfulApplications.Add(CreateSuccessfulApplicant(application, vacancy));
                }
            }
        });

        return successfulApplications.ToList();
    }

    private static SuccessfulApplicant CreateSuccessfulApplicant(VacancyApplication application, VacancyIdentifier vacancy)
    {
        return new SuccessfulApplicant
        {
            ApplicationReviewId = application.ApplicationReviewId,
            CandidateId = application.CandidateId,
            FirstName = application.FirstName,
            LastName = application.LastName,
            DateOfBirth = application.DateOfBirth.GetValueOrDefault(),
            VacancyReference = vacancy.VacancyReference,
        };
    }

    private static List<VacancyApplication> FilterSuccessfulApplicants(List<VacancyApplication> vacancyApplications)
    {
        return vacancyApplications
            .Where(app => app.Status.Equals(ApplicationReviewStatus.Successful))
            .ToList();
    }

    private static List<string> ValidateRequest(GetEmployerSuccessfulApplicantsQuery request)
    {
        const string employerAccountIdRegex = "^[A-Z0-9]{6}$";
        var validationErrors = new List<string>();

        if (string.IsNullOrEmpty(request.EmployerAccountId) || !Regex.IsMatch(request.EmployerAccountId, employerAccountIdRegex,  RegexOptions.None, TimeSpan.FromMilliseconds(100)))
        {
            validationErrors.Add($"Invalid {nameof(request.EmployerAccountId)}");
        }

        return validationErrors;
    }
}