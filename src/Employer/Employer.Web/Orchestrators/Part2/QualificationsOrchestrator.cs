using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class QualificationsOrchestrator
    {
        private readonly IVacancyClient _client;
        private readonly IQualificationsService _qualificationsService;
        
        public QualificationsOrchestrator(IVacancyClient client, IQualificationsService qualificationsService)
        {
            _client = client;
            _qualificationsService = qualificationsService;
        }

        public async Task<QualificationsViewModel> GetQualificationsViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var vm = new QualificationsViewModel
            {
                QualificationTypes = _qualificationsService.GetQualificationTypes(),
                Qualifications = vacancy.Qualifications.ToViewModel()
            };

            return vm;
        }

        public async Task<QualificationsViewModel> GetQualificationsViewModelAsync(QualificationsEditModel m)
        {
            var vm = await GetQualificationsViewModelAsync(m.VacancyId);

            

            return vm;
        }

        public async Task PostQualificationsEditModelAsync(QualificationsEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var qualifications = m.Qualifications?.ToList() ?? new List<QualificationEditModel>();

            if (!string.IsNullOrEmpty(m.RemoveQualification))
            {
                qualifications.RemoveAt(int.Parse(m.RemoveQualification));
            }

            if (!string.IsNullOrWhiteSpace(m.AddQualificationAction))
            {
                qualifications.Add(m);
            }
            
            vacancy.Qualifications = _qualificationsService.SortQualifications(qualifications.ToEntity());
            
            await _client.UpdateVacancyAsync(vacancy, false);
        }
    }
}

