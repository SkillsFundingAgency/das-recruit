using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Api.Models;
using EmployerNameOption = Esfa.Recruit.Vacancies.Client.Domain.Entities.EmployerNameOption;

namespace SFA.DAS.Recruit.Api.Commands
{
    public class CreateVacancyCommandHandler(
        IRecruitVacancyClient recruitVacancyClient,
        IEmployerVacancyClient employerVacancyClient,
        IVacancyRepository vacancyRepository,
        IMessaging messaging,
        ITimeProvider timeProvider,
        ITrainingProviderService trainingProviderService,
        ITrainingProviderSummaryProvider trainingProviderSummaryProvider,
        IProviderVacancyClient providerVacancyClient,
        IProviderRelationshipsService providerRelationshipsService,
        ILocationsService locationsService,
        ILogger<CreateVacancyCommandHandler> logger)
        : IRequestHandler<CreateVacancyCommand, CreateVacancyCommandResponse>
    {
        public async Task<CreateVacancyCommandResponse> Handle(CreateVacancyCommand request, CancellationToken cancellationToken)
        {
            var trainingProvider = await trainingProviderService.GetProviderAsync(request.VacancyUserDetails.Ukprn.Value);

            if (trainingProvider == null)
            {
                return new CreateVacancyCommandResponse
                {
                    ResultCode = ResponseCode.InvalidRequest,
                    ValidationErrors = new List<DetailedValidationError>
                    {
                        new()
                        {
                            Field = nameof(request.VacancyUserDetails.Ukprn), Message = "Training Provider UKPRN not valid"
                        }
                    }.Cast<object>().ToList()
                };
            }

            // additional check to validate the given Training Provider is a Main or Employer Profile with Status not equal to "Not Currently Starting New Apprentices".
            bool isValidTrainingProviderProfile =
                await trainingProviderSummaryProvider.IsTrainingProviderMainOrEmployerProfile(request.VacancyUserDetails.Ukprn.Value);

            if (!isValidTrainingProviderProfile)
            {
                return new CreateVacancyCommandResponse
                {
                    ResultCode = ResponseCode.InvalidRequest,
                    ValidationErrors = new List<DetailedValidationError>
                    {
                        new ()
                        {
                            Field = nameof(request.VacancyUserDetails.Ukprn), Message = "UKPRN of a training provider must be registered to deliver apprenticeship training"
                        }
                    }.Cast<object>().ToList()
                };
            }

            request.Vacancy.TrainingProvider = trainingProvider;

            await LookupAddressesByPostcodeAsync(request.Vacancy);

            var result = recruitVacancyClient.Validate(request.Vacancy, VacancyRuleSet.All);

            if (result.HasErrors)
            {
                return new CreateVacancyCommandResponse
                {
                    ResultCode = ResponseCode.InvalidRequest,
                    ValidationErrors = result.Errors.Select(error => new DetailedValidationError
                    {
                        Field = error.PropertyName,
                        Message = error.ErrorMessage,
                    }).Cast<object>().ToList()
                };
            }

            if (request.ValidateOnly)
            {
                return new CreateVacancyCommandResponse
                {
                    ResultCode = ResponseCode.Created,
                    Data = 1000000001
                };
            }

            try
            {
                await CreateVacancy(request, trainingProvider);
            }
            catch (Exception e)
            {
                logger.LogError(e,"Error creating vacancy");
                return new CreateVacancyCommandResponse
                {
                    ResultCode = ResponseCode.InvalidRequest,
                    ValidationErrors = new List<DetailedValidationError>{new DetailedValidationError
                    {
                        Field = nameof(request.Vacancy.Id), Message = "Unable to create Vacancy. Vacancy already submitted"
                    }}.Cast<object>().ToList()
                };   
            }

            var requiresEmployerApproval = await CheckEmployerApprovalNeeded(request);

            var newVacancy = await MapDraftVacancyValues(request, request.Vacancy, requiresEmployerApproval);

            await vacancyRepository.UpdateAsync(newVacancy);

            await PublishVacancyEvent(requiresEmployerApproval, newVacancy);
            
            return new CreateVacancyCommandResponse
            {
                ResultCode = ResponseCode.Created,
                Data = newVacancy.VacancyReference.Value
            };
        }

        private async Task LookupAddressesByPostcodeAsync(Vacancy vacancy)
        {
            var locations = new List<Address>();
            switch (vacancy.EmployerLocationOption)
            {
                case AvailableWhere.AcrossEngland:
                    return;
                case AvailableWhere.OneLocation:
                case AvailableWhere.MultipleLocations:
                    locations.AddRange(vacancy.EmployerLocations);
                    break;
                default:
                    locations.Add(vacancy.EmployerLocation);
                    break;
            }
            
            var addressesToQuery = locations
                .Where(x => x.Country is null)
                .Select(x => x.Postcode)
                .Distinct()
                .ToList();
            var results = await locationsService.GetBulkPostcodeDataAsync(addressesToQuery);

            locations.ForEach(x =>
            {
                if (x.Country is null && results.TryGetValue(x.Postcode, out var postcodeData))
                {
                    x.Country = postcodeData.Country;
                }
            });
        }

        private async Task CreateVacancy(CreateVacancyCommand request, TrainingProvider trainingProvider)
        {
            if (request.Vacancy.OwnerType == OwnerType.Employer)
            {
                request.VacancyUserDetails.Ukprn = null;
                await employerVacancyClient.CreateEmployerApiVacancy(request.Vacancy.Id, request.Vacancy.Title,
                    request.Vacancy.EmployerAccountId,
                    request.VacancyUserDetails, trainingProvider, request.Vacancy.ProgrammeId);
            }
            else
            {
                await providerVacancyClient.CreateProviderApiVacancy(request.Vacancy.Id, request.Vacancy.Title,
                    request.Vacancy.EmployerAccountId,
                    request.VacancyUserDetails);
            }
        }

        private async Task<bool> CheckEmployerApprovalNeeded(CreateVacancyCommand request)
        {
            if (request.Vacancy.OwnerType == OwnerType.Provider)
            {
                return
                    await providerRelationshipsService.HasProviderGotEmployersPermissionAsync(
                        request.Vacancy.TrainingProvider.Ukprn.Value, request.Vacancy.EmployerAccountId,
                        request.Vacancy.AccountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview );
            }
            return false;
        }

        private async Task<Vacancy> MapDraftVacancyValues(CreateVacancyCommand request, Vacancy draftVacancyFromRequest, bool requiresEmployerReview)
        {
            var newVacancy = await recruitVacancyClient.GetVacancyAsync(draftVacancyFromRequest.Id);

            draftVacancyFromRequest.VacancyReference = newVacancy.VacancyReference;
            draftVacancyFromRequest.TrainingProvider = request.Vacancy.TrainingProvider;
            draftVacancyFromRequest.CreatedByUser = newVacancy.CreatedByUser;
            draftVacancyFromRequest.CreatedDate = newVacancy.CreatedDate;
            draftVacancyFromRequest.OwnerType = request.Vacancy.OwnerType;
            draftVacancyFromRequest.SourceOrigin = newVacancy.SourceOrigin;
            draftVacancyFromRequest.SourceType = newVacancy.SourceType;

            var now = timeProvider.Now;

            if (draftVacancyFromRequest.EmployerNameOption == EmployerNameOption.TradingName)
            {
                var employerProfile = await recruitVacancyClient.GetEmployerProfileAsync(
                    draftVacancyFromRequest.EmployerAccountId,
                    draftVacancyFromRequest.AccountLegalEntityPublicHashedId);
                employerProfile.TradingName = draftVacancyFromRequest.EmployerName;
                await recruitVacancyClient.UpdateEmployerProfileAsync(employerProfile, draftVacancyFromRequest.CreatedByUser);
            }
            
            if (requiresEmployerReview)
            {
                draftVacancyFromRequest.Status = VacancyStatus.Review;
                draftVacancyFromRequest.ReviewDate = now;
                draftVacancyFromRequest.ReviewByUser = request.VacancyUserDetails;
                draftVacancyFromRequest.ReviewByUser = request.VacancyUserDetails;
                draftVacancyFromRequest.ReviewCount += 1;
            }
            else
            {
                draftVacancyFromRequest.Status = VacancyStatus.Submitted;
                draftVacancyFromRequest.SubmittedDate = now;
                draftVacancyFromRequest.SubmittedByUser = request.VacancyUserDetails;    
            }
            
            draftVacancyFromRequest.LastUpdatedDate = now;
            draftVacancyFromRequest.LastUpdatedByUser = request.VacancyUserDetails;
            draftVacancyFromRequest.AdditionalQuestion1 = request.Vacancy.AdditionalQuestion1;
            draftVacancyFromRequest.AdditionalQuestion2 = request.Vacancy.AdditionalQuestion2;
            return draftVacancyFromRequest;
        }

        private async Task PublishVacancyEvent(bool requiresEmployerApproval, Vacancy newVacancy)
        {
            if (requiresEmployerApproval)
            {
                await messaging.PublishEvent(new VacancyReviewedEvent
                {
                    EmployerAccountId = newVacancy.EmployerAccountId,
                    VacancyId = newVacancy.Id,
                    VacancyReference = newVacancy.VacancyReference.Value,
                    Ukprn = newVacancy.TrainingProvider.Ukprn.GetValueOrDefault()
                });
            }
            else
            {
                await messaging.PublishEvent(new VacancySubmittedEvent
                {
                    EmployerAccountId = newVacancy.EmployerAccountId,
                    VacancyId = newVacancy.Id,
                    VacancyReference = newVacancy.VacancyReference.Value
                });
            }
        }
    }
}