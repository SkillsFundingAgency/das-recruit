using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.SearchResultPreview;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
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
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            if (!vacancy.EmployerAccountId.Equals(vrm.EmployerAccountId, StringComparison.OrdinalIgnoreCase))
                throw new AuthorisationException(string.Format(ExceptionMessages.VacancyUnauthorisedAccess, vrm.EmployerAccountId, vacancy.EmployerAccountId, vacancy.Title, vacancy.Id));

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            
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
                HasYearlyWage = (vacancy.Wage != null && vacancy.Wage.WageType != WageType.Unspecified)
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
