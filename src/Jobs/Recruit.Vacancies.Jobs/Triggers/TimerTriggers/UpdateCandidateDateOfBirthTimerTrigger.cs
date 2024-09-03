using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.TimerTriggers;

public class UpdateCandidateDateOfBirthTimerTrigger
{
    private readonly ILogger<UpdateCandidateDateOfBirthTimerTrigger> _logger;
    private readonly IApplicationReviewRepository _applicationReviewRepository;
    private readonly IOuterApiClient _outerApiClient;

    public UpdateCandidateDateOfBirthTimerTrigger(ILogger<UpdateCandidateDateOfBirthTimerTrigger> logger, IApplicationReviewRepository applicationReviewRepository, IOuterApiClient outerApiClient)
    {
        _logger = logger;
        _applicationReviewRepository = applicationReviewRepository;
        _outerApiClient = outerApiClient;
    }
    public async Task UpdateCandidateDateOfBirth([TimerTrigger(Schedules.Hourly)] TimerInfo timerInfo, TextWriter log)
    {
        _logger.LogInformation($"Timer trigger {this.GetType().Name} fired");

        var candidateApplications = await _applicationReviewRepository.GetWithoutDateOfBirthAsync();
        foreach (var application in candidateApplications)
        {
            var candidate =
                await _outerApiClient.Get<GetCandidateDetailApiResponse.GetCandidateApplicationApiResponse>(
                    new GetCandidateDetailApiRequest(application.CandidateId));
            if (candidate != null)
            {
                application.Application.BirthDate = candidate.DateOfBirth;
                await _applicationReviewRepository.UpdateAsync(application);
            }
        }

        if (candidateApplications.Count == 0)
        {
            _logger.LogInformation($"All applicant DOBs up to date");
        }
    }
}