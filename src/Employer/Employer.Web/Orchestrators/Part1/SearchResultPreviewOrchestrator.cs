using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.SearchResultPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class SearchResultPreviewOrchestrator
    {
        private readonly IVacancyClient _client;
        private readonly IGeocodeImageService _mapService;

        public SearchResultPreviewOrchestrator(IVacancyClient client, IGeocodeImageService mapService)
        {
            _client = client;
            _mapService = mapService;
        }
        
        public async Task<SearchResultPreviewViewModel> GetSearchResultPreviewViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var vm = new SearchResultPreviewViewModel
            {
                EmployerName = vacancy.EmployerName,
                NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
                ShortDescription = vacancy.ShortDescription,
                ClosingDate = vacancy.ClosingDate?.AsDisplayDate(),
                StartDate = vacancy.StartDate?.AsDisplayDate(),
                LevelName = vacancy.Programme?.LevelName,
                Title = vacancy.Title,
                Wage = vacancy.Wage?.ToText()
            };

            if (vacancy.EmployerLocation != null)
            {
                vm.MapUrl = vacancy.EmployerLocation.HasGeocode
                    ? _mapService.GetMapImageUrl(vacancy.EmployerLocation.Latitude.ToString(), vacancy.EmployerLocation.Longitude.ToString())
                    : _mapService.GetMapImageUrl(vacancy.EmployerLocation?.Postcode);
            }
            
            return vm;
        }
    }
}
