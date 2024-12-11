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

namespace SFA.DAS.Recruit.Api.Queries;

public class GetEmployerSummaryQueryHandler(IQueryStoreReader queryStoreReader, ILogger<GetApplicantsQueryHandler> logger)
    : IRequestHandler<GetEmployerSummaryQuery, GetEmployerSummaryResponse>
{
    private readonly ILogger<GetApplicantsQueryHandler> _logger = logger;

    public async Task<GetEmployerSummaryResponse> Handle(GetEmployerSummaryQuery request, CancellationToken cancellationToken)
    {
        var validationErrors = ValidateRequest(request);

        if (validationErrors.Count != 0)
        {
            return new GetEmployerSummaryResponse { ResultCode = ResponseCode.InvalidRequest, ValidationErrors = validationErrors.Cast<object>().ToList() };
        }

        var dashboard = await queryStoreReader.GetEmployerDashboardAsync(request.EmployerAccountId);

        if (dashboard == null)
        {
            return new GetEmployerSummaryResponse { ResultCode = ResponseCode.NotFound };
        }

        return new GetEmployerSummaryResponse
        {
            ResultCode = ResponseCode.Success,
            Data = EmployerAccountSummaryMapper.MapFromEmployerDashboard(dashboard, request.EmployerAccountId)
        };
    }

    private static List<string> ValidateRequest(GetEmployerSummaryQuery request)
    {
        const string employerAccountIdFieldName = nameof(request.EmployerAccountId);
        const string employerAccountIdRegex = @"^[A-Z0-9]{6}$";
        var validationErrors = new List<string>();

        if (string.IsNullOrEmpty(request.EmployerAccountId) || Regex.IsMatch(request.EmployerAccountId, employerAccountIdRegex) == false)
        {
            validationErrors.Add($"Invalid {FieldNameHelper.ToCamelCasePropertyName(employerAccountIdFieldName)}");
        }

        return validationErrors;
    }
}