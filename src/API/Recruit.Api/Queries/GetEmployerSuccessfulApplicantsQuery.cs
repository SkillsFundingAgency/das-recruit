using System;
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
    public async Task<GetEmployerSuccessfulApplicantsQueryResponse> Handle(GetEmployerSuccessfulApplicantsQuery request, CancellationToken cancellationToken)
    {
        var validationErrors = ValidateRequest(request);

        if (validationErrors.Count != 0)
        {
            return new GetEmployerSuccessfulApplicantsQueryResponse { ResultCode = ResponseCode.InvalidRequest, ValidationErrors = validationErrors.Cast<object>().ToList() };
        }

        List<Task<VacancyApplications>> applicationsTasks = [];
        
        try
        {
            var vacancies = await vacancyQuery.GetVacanciesByEmployerAccountAsync<Vacancy>(request.EmployerAccountId);
            
            foreach (var vacancy in vacancies)
            {
                applicationsTasks.Add(queryStoreReader.GetVacancyApplicationsAsync(vacancy.VacancyReference.ToString()));
            }

            await Task.WhenAll(applicationsTasks);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        // get the employer vacancies
        
        var successfulApplications = new List<VacancyApplication>();

        foreach (var task in applicationsTasks)
        {
            successfulApplications.AddRange(FilterSuccessfulApplicants(task.Result));
        }

        return new GetEmployerSuccessfulApplicantsQueryResponse
        {
            ResultCode = ResponseCode.Success,
            Data = successfulApplications.Select(ApplicantSummaryMapper.MapFromVacancyApplication)
        };
    }

    private static List<VacancyApplication> FilterSuccessfulApplicants(VacancyApplications vacancyApplications)
    {
        return vacancyApplications.Applications
            .Where(app => app.Status.ToString().Equals(ApplicationReviewStatus.Successful.ToString(), StringComparison.InvariantCultureIgnoreCase))
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