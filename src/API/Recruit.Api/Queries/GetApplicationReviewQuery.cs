using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;

using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Queries;

public record GetApplicationReviewQuery(Guid CandidateId, Guid ApplicationReviewId) : IRequest<GetApplicationReviewResponse>
{
    public Guid ApplicationReviewId { get; set; } = ApplicationReviewId;
    public Guid CandidateId { get; set; } = CandidateId;
}

public class GetApplicationReviewQueryHandler(IApplicationReviewRepository applicationReviewRepository) : IRequestHandler<GetApplicationReviewQuery, GetApplicationReviewResponse>
{
    public async Task<GetApplicationReviewResponse> Handle(GetApplicationReviewQuery request, CancellationToken cancellationToken)
    {
        var validationErrors = ValidateRequest(request);

        if (validationErrors.Count != 0)
        {
            return new GetApplicationReviewResponse
            {
                ResultCode = ResponseCode.InvalidRequest,
                ValidationErrors = validationErrors.Cast<object>().ToList()
            };
        }

        var applicationReview = await applicationReviewRepository.GetAsync(request.ApplicationReviewId);

        if (applicationReview == null)
        {
            return new GetApplicationReviewResponse
            {
                ResultCode = ResponseCode.NotFound
            };
        }

        if (applicationReview.CandidateId != request.CandidateId)
        {
            throw new Exception("The candidateId in the request does not match the candidateId in the applicationReview.");
        }

        return new GetApplicationReviewResponse
        {
            ResultCode = ResponseCode.Success,
            Data = new ApplicationReviewResponse
            {
                ApplicationReviewId = applicationReview.Id,
                ApplicationDate = applicationReview.Application.ApplicationDate,
                VacancyReference = applicationReview.VacancyReference,
                CandidateId = applicationReview.CandidateId,

                BirthDate = applicationReview.Application.BirthDate,
                FirstName = applicationReview.Application.FirstName,
                LastName = applicationReview.Application.LastName,

                Email = applicationReview.Application.Email,
                Phone = applicationReview.Application.Phone,

                AddressLine1 = applicationReview.Application.AddressLine1,
                AddressLine2 = applicationReview.Application.AddressLine2,
                AddressLine3 = applicationReview.Application.AddressLine3,
                AddressLine4 = applicationReview.Application.AddressLine4,
                Postcode = applicationReview.Application.Postcode,
            }
        };
    }

    private static List<string> ValidateRequest(GetApplicationReviewQuery request)
    {
        var validationErrors = new List<string>();
        
        if (request.CandidateId == Guid.Empty)
        {
            validationErrors.Add($"Invalid {nameof(request.CandidateId)}");
        }

        if (request.ApplicationReviewId == Guid.Empty)
        {
            validationErrors.Add($"Invalid {nameof(request.ApplicationReviewId)}");
        }

        return validationErrors;
    }
}