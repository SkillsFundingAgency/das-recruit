using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.SearchResultPreview;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class SearchResultPreviewOrchestrator
    {
        private const int MapImageWidth = 190;
        private const int MapImageHeight = 125;
        private readonly IEmployerVacancyClient _client;
        private readonly IGeocodeImageService _mapService;
        private readonly IGetMinimumWages _wageService;

        public SearchResultPreviewOrchestrator(IEmployerVacancyClient client, IGeocodeImageService mapService, IGetMinimumWages wageService)
        {
            _client = client;
            _mapService = mapService;
            _wageService = wageService;
        }
        
        public async Task<SearchResultPreviewViewModel> GetSearchResultPreviewViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.SearchResultPreview_Get);

            var vm = new SearchResultPreviewViewModel
            {
                EmployerName = vacancy.EmployerName,
                NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
                ShortDescription = vacancy.ShortDescription,
                ClosingDate = vacancy.ClosingDate?.AsDisplayDate(),
                StartDate = vacancy.StartDate?.AsDisplayDate(),
                LevelName = await GetLevelName(vacancy.ProgrammeId),
                Title = vacancy.Title,
                Wage = vacancy.Wage?.ToText(
                    () => _wageService.GetNationalMinimumWageRange(vacancy.StartDate.Value),
                    () => _wageService.GetApprenticeNationalMinimumWage(vacancy.StartDate.Value)),
                HasYearlyWage = (vacancy.Wage != null && vacancy.Wage.WageType != WageType.Unspecified),
                IsDisabilityConfident = vacancy.IsDisabilityConfident
            };

            if (vacancy.EmployerLocation != null)
            {
                vm.MapUrl = vacancy.EmployerLocation.HasGeocode
                    ? _mapService.GetMapImageUrl(vacancy.EmployerLocation.Latitude.ToString(), vacancy.EmployerLocation.Longitude.ToString(), MapImageWidth, MapImageHeight)
                    : _mapService.GetMapImageUrl(vacancy.EmployerLocation?.Postcode, MapImageWidth, MapImageHeight);
            }
            
            return vm;
        }

        private async Task<string> GetLevelName(string programmeId)
        {
            if (string.IsNullOrWhiteSpace(programmeId))
                return null;

            var match = await _client.GetApprenticeshipProgrammeAsync(programmeId);

            return match?.Level.GetDisplayName();
        }
    }
}
