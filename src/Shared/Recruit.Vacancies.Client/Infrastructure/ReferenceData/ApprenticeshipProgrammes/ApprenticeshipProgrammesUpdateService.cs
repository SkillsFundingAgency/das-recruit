using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Microsoft.Extensions.Logging;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammesUpdateService : IApprenticeshipProgrammesUpdateService
    {
        private const int AcceptablePercentage = 10;
        private readonly ILogger<ApprenticeshipProgrammesUpdateService> _logger;
        private readonly IOuterApiClient _outerApiClient;
        private readonly IReferenceDataWriter _referenceDataWriter;
        private readonly IReferenceDataReader _referenceDataReader;

        public ApprenticeshipProgrammesUpdateService(
            ILogger<ApprenticeshipProgrammesUpdateService> logger,
            IReferenceDataWriter referenceDataWriter,
            IReferenceDataReader referenceDataReader,
            IOuterApiClient outerApiClient)
        {
            _logger = logger;
            _referenceDataWriter = referenceDataWriter;
            _referenceDataReader = referenceDataReader;
            _outerApiClient = outerApiClient;
        }

        public async Task UpdateApprenticeshipProgrammesAsync()
        {
            try
            {
                var trainingProgrammesTask = await GetTrainingProgrammes();

                var trainingProgrammesFromApi = trainingProgrammesTask.ToList();

                var standardsCount = trainingProgrammesFromApi.Count(c=>c.ApprenticeshipType == TrainingType.Standard);
                if (standardsCount == 0)
                    throw new InfrastructureException("Retrieved 0 standards from the apprenticeships api.");

                var frameworksCount = trainingProgrammesFromApi.Count(c=>c.ApprenticeshipType == TrainingType.Framework);
                if (frameworksCount == 0)
                    throw new InfrastructureException("Retrieved 0 frameworks from the apprenticeships api.");

                
                await ValidateList(trainingProgrammesFromApi);                
                await _referenceDataWriter.UpsertReferenceData(new ApprenticeshipProgrammes {
                        Data = trainingProgrammesFromApi.Distinct(new ApprenticeshipProgrammeEqualityComparer()).ToList()
                    });
                _logger.LogInformation("Inserted: {standardCount} standards and {frameworkCount} frameworks.", standardsCount, frameworksCount );                                
            }
            catch (Exception e)
            {
                
                _logger.LogError(e, "Failed to get training programmes from api");
                
                throw;
            }
        }

        private async Task ValidateList(List<ApprenticeshipProgramme> apprenticeshipProgrammesFromApi)
        {
            var apprenticeshipProgrammesFromDb = await _referenceDataReader.GetReferenceData<ApprenticeshipProgrammes>();

            if (apprenticeshipProgrammesFromDb == null)
                return;

            var apiCount = apprenticeshipProgrammesFromApi.Count;
            var dbCount = apprenticeshipProgrammesFromDb.Data.Count();
            var difference = Math.Abs(apiCount - dbCount);
            double percent = (double)(difference * 100) / apiCount; 
            if (percent > AcceptablePercentage)
            {
                _logger.LogWarning("There is a difference of more than 10% between the existing ApprenticeshipProgrammes and the " +
                                   $"new ApprenticeshipProgramme, apprenticeshipProgrammesFromApi:{apiCount} and " +
                                   $"apprenticeshipProgrammesFromDb:{dbCount}");                
            }            
        }

        private async Task<IEnumerable<ApprenticeshipProgramme>> GetTrainingProgrammes()
        {
            _logger.LogTrace("Getting Training Programmes from Outer Api");

            var retryPolicy = GetApiRetryPolicy();

            var result = await retryPolicy.ExecuteAsync(context => _outerApiClient.Get<GetTrainingProgrammesResponse>(new GetTrainingProgrammesRequest()), new Dictionary<string, object>() {{ "apiCall", "Standards" }});

            return result.TrainingProgrammes.Select(c=>(ApprenticeshipProgramme)c);

        }

        private Polly.Retry.RetryPolicy GetApiRetryPolicy()
        {
            return Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(4)
                    }, (exception, timeSpan, retryCount, context) => {
                        _logger.LogWarning($"Error connecting to Outer Api for {context["apiCall"]}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");    
                    });
        }
    }
}