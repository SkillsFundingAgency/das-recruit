using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
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

        public PreviewOrchestrator(IVacancyClient client)
        {
            _client = client;
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
                Latitude = "52.4001929857", //temporary stub!!
                Longitude = "-1.9689295778",  //temporary stub!!
            };
            
            return vm;
        }
    }
}
