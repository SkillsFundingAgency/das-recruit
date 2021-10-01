using System;
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
    public class GetProviderOrganisationStatusQueryHandler : IRequestHandler<GetProviderOrganisationStatusQuery, GetOrganisationStatusResponse>
    {
        private readonly ILogger<GetProviderOrganisationStatusQueryHandler> _logger;
        private readonly IQueryStoreReader _queryStoreReader;

        public GetProviderOrganisationStatusQueryHandler(ILogger<GetProviderOrganisationStatusQueryHandler> logger, IQueryStoreReader queryStoreReader)
        {
            _logger = logger;
            _queryStoreReader = queryStoreReader;
        }

        public async Task<GetOrganisationStatusResponse> Handle(GetProviderOrganisationStatusQuery request, CancellationToken cancellationToken)
        {
            var validationErrors = ValidateRequest(request);

            if (validationErrors.Any())
            {
                return new GetOrganisationStatusResponse { ResultCode = ResponseCode.InvalidRequest, ValidationErrors = validationErrors };
            }

            var blockedProviders = await _queryStoreReader.GetBlockedProviders();

            if (blockedProviders == null)
            {
                return new GetOrganisationStatusResponse { ResultCode = ResponseCode.NotFound };
            }

            var status = blockedProviders.Data.Any(bo => bo.BlockedOrganisationId == request.Ukprn.ToString()) ? BlockStatus.Blocked : BlockStatus.NotBlocked;
            return new GetOrganisationStatusResponse { ResultCode = ResponseCode.Success, Data = new OrganisationStatus(status) };
        }

        private List<string> ValidateRequest(GetProviderOrganisationStatusQuery request)
        {
            const string ukprnFieldName = nameof(request.Ukprn);
            const string ukprnRegex = @"^\d{8}$";
            var validationErrors = new List<string>();

            if (Regex.IsMatch(request.Ukprn.ToString(), ukprnRegex) == false)
            {
                validationErrors.Add($"Invalid {FieldNameHelper.ToCamelCasePropertyName(ukprnFieldName)}");
            }

            return validationErrors;
        }
    }
}