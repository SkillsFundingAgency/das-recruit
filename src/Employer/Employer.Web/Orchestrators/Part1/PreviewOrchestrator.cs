using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Preview;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MongoDB.Bson.Serialization.Serializers;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class PreviewOrchestrator
    {

        private const string dateFormat = "d MMM yyyy";

        private readonly IVacancyClient _client;
        private readonly IGeocodeImageService _mapService;

        public PreviewOrchestrator(IVacancyClient client, IGeocodeImageService mapService)
        {
            _client = client;
            _mapService = mapService;
        }
        
        public async Task<PreviewViewModel> GetPreviewViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var vm = new PreviewViewModel
            {
                OrganisationName = vacancy.OrganisationName,
                NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
                ShortDescription = vacancy.ShortDescription,
                ClosingDate = vacancy.ClosingDate?.ToString(dateFormat),
                StartDate = vacancy.StartDate?.ToString(dateFormat),
                LevelName = vacancy.Programme?.LevelName,
                Wage = vacancy.Wage?.ToText(),
                MapUrl = vacancy.Location.HasGeocode ?
                    _mapService.GetMapImageUrl(vacancy.Location.Latitude, vacancy.Location.Longitude) : 
                    _mapService.GetMapImageUrl(vacancy.Location?.Postcode)
            };
            
            return vm;
        }
    }
}
