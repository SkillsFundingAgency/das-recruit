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
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Commands
{
    public class CreateVacancyCommandHandler : IRequestHandler<CreateVacancyCommand, CreateVacancyCommandResponse>
    {
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IEmployerVacancyClient _employerVacancyClient;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly IProviderVacancyClient _providerVacancyClient;

        public CreateVacancyCommandHandler (
            IRecruitVacancyClient recruitVacancyClient, 
            IEmployerVacancyClient employerVacancyClient, 
            IVacancyRepository vacancyRepository, 
            IMessaging messaging,
            ITimeProvider timeProvider,
            ITrainingProviderService trainingProviderService,
            IProviderVacancyClient providerVacancyClient)
        {
            _recruitVacancyClient = recruitVacancyClient;
            _employerVacancyClient = employerVacancyClient;
            _vacancyRepository = vacancyRepository;
            _messaging = messaging;
            _timeProvider = timeProvider;
            _trainingProviderService = trainingProviderService;
            _providerVacancyClient = providerVacancyClient;
        }
        public async Task<CreateVacancyCommandResponse> Handle(CreateVacancyCommand request, CancellationToken cancellationToken)
        {
            var trainingProvider = await _trainingProviderService.GetProviderAsync(request.VacancyUserDetails.Ukprn.Value);

            if (trainingProvider == null)
            {
                return new CreateVacancyCommandResponse
                {
                    ResultCode = ResponseCode.InvalidRequest,
                    ValidationErrors = new List<string>{"Training Provider UKPRN not valid"}
                };
            }

            request.Vacancy.TrainingProvider = trainingProvider;
            request.Vacancy.OwnerType = string.IsNullOrEmpty(request.VacancyUserDetails.Email) ? OwnerType.Provider : OwnerType.Employer; 
            
            
            var result = _recruitVacancyClient.Validate(request.Vacancy, VacancyRuleSet.All);

            if (result.HasErrors)
            {
                return new CreateVacancyCommandResponse
                {
                    ResultCode = ResponseCode.InvalidRequest,
                    ValidationErrors = result.Errors.Select(c=>c.ErrorMessage).ToList()
                };    
            }

            try
            {
                if (!string.IsNullOrEmpty(request.VacancyUserDetails.Email))
                {
                    request.VacancyUserDetails.Ukprn = null;
                    await _employerVacancyClient.CreateEmployerApiVacancy(request.Vacancy.Id, request.Vacancy.Title, request.Vacancy.EmployerAccountId,
                        request.VacancyUserDetails, trainingProvider, request.Vacancy.ProgrammeId);    
                }
                else
                {
                    await _providerVacancyClient.CreateProviderApiVacancy(request.Vacancy.Id, request.Vacancy.Title,
                        request.Vacancy.EmployerAccountId,
                        request.VacancyUserDetails);
                }
            }
            catch (Exception)
            {
                return new CreateVacancyCommandResponse
                {
                    ResultCode = ResponseCode.InvalidRequest,
                    ValidationErrors = new List<string>{"Unable to create Vacancy. Vacancy already submitted"}
                };   
            }
            
            var newVacancy = await MapDraftVacancyValues(request, request.Vacancy);

            await _vacancyRepository.UpdateAsync(newVacancy);
            
            await _messaging.PublishEvent(new VacancySubmittedEvent
            {
                EmployerAccountId = newVacancy.EmployerAccountId,
                VacancyId = newVacancy.Id,
                VacancyReference = newVacancy.VacancyReference.Value
            });
            
            return new CreateVacancyCommandResponse
            {
                ResultCode = ResponseCode.Created,
                Data = newVacancy.VacancyReference.Value
            };
        }

        private async Task<Vacancy> MapDraftVacancyValues(CreateVacancyCommand request, Vacancy draftVacancyFromRequest)
        {
            var newVacancy = await _recruitVacancyClient.GetVacancyAsync(draftVacancyFromRequest.Id);

            draftVacancyFromRequest.VacancyReference = newVacancy.VacancyReference;
            draftVacancyFromRequest.TrainingProvider = request.Vacancy.TrainingProvider;
            draftVacancyFromRequest.CreatedByUser = newVacancy.CreatedByUser;
            draftVacancyFromRequest.CreatedDate = newVacancy.CreatedDate;
            draftVacancyFromRequest.OwnerType = newVacancy.OwnerType;
            draftVacancyFromRequest.SourceOrigin = newVacancy.SourceOrigin;
            draftVacancyFromRequest.SourceType = newVacancy.SourceType;
            
            var now = _timeProvider.Now;

            draftVacancyFromRequest.Status = VacancyStatus.Submitted;
            draftVacancyFromRequest.SubmittedDate = now;
            draftVacancyFromRequest.SubmittedByUser = request.VacancyUserDetails;
            draftVacancyFromRequest.LastUpdatedDate = now;
            draftVacancyFromRequest.LastUpdatedByUser = request.VacancyUserDetails;
            return draftVacancyFromRequest;
        }
    }
}