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
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Api.Models;
using EmployerNameOption = Esfa.Recruit.Vacancies.Client.Domain.Entities.EmployerNameOption;

namespace SFA.DAS.Recruit.Api.Commands
{
    public class CreateTraineeshipVacancyCommandHandler : IRequestHandler<CreateTraineeshipVacancyCommand, CreateTraineeshipVacancyCommandResponse>
    {
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly ILogger<CreateTraineeshipVacancyCommandHandler> _logger;

        public CreateTraineeshipVacancyCommandHandler(
            IRecruitVacancyClient recruitVacancyClient,
            IVacancyRepository vacancyRepository,
            IMessaging messaging,
            ITimeProvider timeProvider,
            ITrainingProviderService trainingProviderService,
            IProviderVacancyClient providerVacancyClient,
            ILogger<CreateTraineeshipVacancyCommandHandler> logger)
        {
            _recruitVacancyClient = recruitVacancyClient;
            _vacancyRepository = vacancyRepository;
            _messaging = messaging;
            _timeProvider = timeProvider;
            _trainingProviderService = trainingProviderService;
            _providerVacancyClient = providerVacancyClient;
            _logger = logger;
        }
        public async Task<CreateTraineeshipVacancyCommandResponse> Handle(CreateTraineeshipVacancyCommand request, CancellationToken cancellationToken)
        {
            var trainingProvider = await _trainingProviderService.GetProviderAsync(request.VacancyUserDetails.Ukprn.Value);

            if (trainingProvider == null)
            {
                return new CreateTraineeshipVacancyCommandResponse
                {
                    ResultCode = ResponseCode.InvalidRequest,
                    ValidationErrors = new List<DetailedValidationError>
                    {
                        new DetailedValidationError
                        {
                            Field = nameof(request.VacancyUserDetails.Ukprn), Message = "Training Provider UKPRN not valid"
                        }
                    }.Cast<object>().ToList()
                };
            }

            request.Vacancy.TrainingProvider = trainingProvider;
            request.Vacancy.OwnerType = OwnerType.Provider;


            var result = _recruitVacancyClient.Validate(request.Vacancy, VacancyRuleSet.All);

            if (result.HasErrors)
            {
                return new CreateTraineeshipVacancyCommandResponse
                {
                    ResultCode = ResponseCode.InvalidRequest,
                    ValidationErrors = result.Errors.Select(error => new DetailedValidationError
                    {
                        Field = error.PropertyName,
                        Message = error.ErrorMessage
                    }).Cast<object>().ToList()
                };
            }

            if (request.ValidateOnly)
            {
                return new CreateTraineeshipVacancyCommandResponse
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
                _logger.LogError(e, "Error creating vacancy");
                return new CreateTraineeshipVacancyCommandResponse
                {
                    ResultCode = ResponseCode.InvalidRequest,
                    ValidationErrors = new List<DetailedValidationError>{new DetailedValidationError
                    {
                        Field = nameof(request.Vacancy.Id), Message = "Unable to create Vacancy. Vacancy already submitted"
                    }}.Cast<object>().ToList()
                };
            }

            var newVacancy = await MapDraftVacancyValues(request, request.Vacancy);

            await _vacancyRepository.UpdateAsync(newVacancy);

            await PublishVacancyEvent(newVacancy);

            return new CreateTraineeshipVacancyCommandResponse
            {
                ResultCode = ResponseCode.Created,
                Data = newVacancy.VacancyReference.Value
            };
        }

        private async Task CreateVacancy(CreateTraineeshipVacancyCommand request, TrainingProvider trainingProvider)
        {
            await _providerVacancyClient.CreateProviderApiVacancy(request.Vacancy.Id, request.Vacancy.Title,
              request.Vacancy.EmployerAccountId,
              request.VacancyUserDetails);

        }

        private async Task<Vacancy> MapDraftVacancyValues(CreateTraineeshipVacancyCommand request, Vacancy draftVacancyFromRequest)
        {
            var newVacancy = await _recruitVacancyClient.GetVacancyAsync(draftVacancyFromRequest.Id);

            draftVacancyFromRequest.VacancyReference = newVacancy.VacancyReference;
            draftVacancyFromRequest.TrainingProvider = request.Vacancy.TrainingProvider;
            draftVacancyFromRequest.CreatedByUser = newVacancy.CreatedByUser;
            draftVacancyFromRequest.CreatedDate = newVacancy.CreatedDate;
            draftVacancyFromRequest.OwnerType = newVacancy.OwnerType;
            draftVacancyFromRequest.SourceOrigin = newVacancy.SourceOrigin;
            draftVacancyFromRequest.SourceType = newVacancy.SourceType;
            draftVacancyFromRequest.VacancyType = newVacancy.VacancyType;

            var now = _timeProvider.Now;

            if (draftVacancyFromRequest.EmployerNameOption == EmployerNameOption.TradingName)
            {
                var employerProfile = await _recruitVacancyClient.GetEmployerProfileAsync(
                    draftVacancyFromRequest.EmployerAccountId,
                    draftVacancyFromRequest.AccountLegalEntityPublicHashedId);
                employerProfile.TradingName = draftVacancyFromRequest.EmployerName;
                await _recruitVacancyClient.UpdateEmployerProfileAsync(employerProfile, draftVacancyFromRequest.CreatedByUser);
            }

            draftVacancyFromRequest.Status = VacancyStatus.Submitted;
            draftVacancyFromRequest.SubmittedDate = now;
            draftVacancyFromRequest.SubmittedByUser = request.VacancyUserDetails;

            draftVacancyFromRequest.LastUpdatedDate = now;
            draftVacancyFromRequest.LastUpdatedByUser = request.VacancyUserDetails;
 
            return draftVacancyFromRequest;
        }

        private async Task PublishVacancyEvent(Vacancy newVacancy)
        {
            await _messaging.PublishEvent(new VacancySubmittedEvent
            {
                EmployerAccountId = newVacancy.EmployerAccountId,
                VacancyId = newVacancy.Id,
                VacancyReference = newVacancy.VacancyReference.Value
            });
        }
    }
}