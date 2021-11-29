using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Api.Helpers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Services;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetEmployerOrganisationStatusQueryHandler : IRequestHandler<GetEmployerOrganisationStatusQuery, GetOrganisationStatusResponse>
    {
        private readonly ILogger<GetEmployerOrganisationStatusQueryHandler> _logger;
        private readonly IQueryStoreReader _queryStoreReader;

        public GetEmployerOrganisationStatusQueryHandler(ILogger<GetEmployerOrganisationStatusQueryHandler> logger, IQueryStoreReader queryStoreReader)
        {
            _logger = logger;
            _queryStoreReader = queryStoreReader;
        }

        public async Task<GetOrganisationStatusResponse> Handle(GetEmployerOrganisationStatusQuery request, CancellationToken cancellationToken)
        {
            var validationErrors = ValidateRequest(request);

            if (validationErrors.Any())
            {
                return new GetOrganisationStatusResponse { ResultCode = ResponseCode.InvalidRequest, ValidationErrors = validationErrors.Cast<object>().ToList() };
            }

            var blockedEmployers = await _queryStoreReader.GetBlockedEmployers();

            if (blockedEmployers == null)
            {
                return new GetOrganisationStatusResponse { ResultCode = ResponseCode.NotFound };
            }

            var status = blockedEmployers.Data.Any(bo => bo.BlockedOrganisationId.Equals(request.EmployerAccountId)) ? BlockStatus.Blocked : BlockStatus.NotBlocked;
            return new GetOrganisationStatusResponse { ResultCode = ResponseCode.Success, Data = new OrganisationStatus(status) };
        }

        private List<string> ValidateRequest(GetEmployerOrganisationStatusQuery request)
        {
            const string employerAccountIdFieldName = nameof(request.EmployerAccountId);
            const string employerAccountIdRegex = @"^[A-Z0-9]{6}$";

            var validationErrors = new List<string>();

            if (Regex.IsMatch(request.EmployerAccountId, employerAccountIdRegex) == false)
            {
                validationErrors.Add($"Invalid {FieldNameHelper.ToCamelCasePropertyName(employerAccountIdFieldName)}");
            }

            return validationErrors;
        }
    }
}