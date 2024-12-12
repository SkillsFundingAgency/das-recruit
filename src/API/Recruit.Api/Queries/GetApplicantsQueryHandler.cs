using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Api.Helpers;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Services;

namespace SFA.DAS.Recruit.Api.Queries;

public class GetApplicantsQueryHandler(ILogger<GetApplicantsQueryHandler> logger, IQueryStoreReader queryStoreReader)
    : IRequestHandler<GetApplicantsQuery, GetApplicantsResponse>
{
    private readonly ILogger<GetApplicantsQueryHandler> _logger = logger;
    private const long ValidMinimumRecruitVacancyReferenceNumber = 1000000000;

    private IEnumerable<string> _validFilters = [nameof(ApplicationReviewStatus.Successful), nameof(ApplicationReviewStatus.Unsuccessful)];

    public async Task<GetApplicantsResponse> Handle(GetApplicantsQuery request, CancellationToken cancellationToken)
    {
        var validationErrors = ValidateRequest(request);

        if (validationErrors.Count != 0)
        {
            return new GetApplicantsResponse { ResultCode = ResponseCode.InvalidRequest, ValidationErrors = validationErrors.Cast<object>().ToList() };
        }

        var vacancyApplications = await queryStoreReader.GetVacancyApplicationsAsync(request.VacancyReference.ToString());

        if (vacancyApplications == null || vacancyApplications.Applications.Any() == false)
        {
            return new GetApplicantsResponse { ResultCode = ResponseCode.NotFound };
        }

        var applicants = FilterApplicants(request.ApplicantApplicationOutcomeFilter, vacancyApplications);

        if (applicants.Count != 0 == false)
        {
            return new GetApplicantsResponse { ResultCode = ResponseCode.NotFound };
        }

        return new GetApplicantsResponse
        {
            ResultCode = ResponseCode.Success,
            Data = applicants.Select(ApplicantSummaryMapper.MapFromVacancyApplication)
        };
    }

    private static List<VacancyApplication> FilterApplicants(string applicantApplicationOutcomeFilter, VacancyApplications vacancyApplications)
    {
        var applicants = vacancyApplications.Applications;

        if (string.IsNullOrEmpty(applicantApplicationOutcomeFilter) == false)
        {
            applicants = applicants
                .Where(app => app.Status.ToString().Equals(applicantApplicationOutcomeFilter, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }

        return applicants;
    }

    private List<string> ValidateRequest(GetApplicantsQuery request)
    {
        const string vacancyReferenceFieldName = nameof(request.VacancyReference);
        const string applicantApplicationOutcomeFilterFieldName = nameof(request.ApplicantApplicationOutcomeFilter);
        var validationErrors = new List<string>();

        if (request.VacancyReference < ValidMinimumRecruitVacancyReferenceNumber)
        {
            validationErrors.Add($"Invalid {FieldNameHelper.ToCamelCasePropertyName(vacancyReferenceFieldName)}");
        }

        if (!string.IsNullOrEmpty(request.ApplicantApplicationOutcomeFilter)
            && _validFilters.All(filter => filter.Equals(request.ApplicantApplicationOutcomeFilter, StringComparison.InvariantCultureIgnoreCase) == false))
        {
            validationErrors.Add($"Invalid {FieldNameHelper.ToCamelCasePropertyName(applicantApplicationOutcomeFilterFieldName)}");
        }

        return validationErrors;
    }
}