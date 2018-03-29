using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class QualificationsOrchestrator
    {
        private readonly IVacancyClient _client;
        private readonly QualificationsConfiguration _qualificationsConfig;

        public QualificationsOrchestrator(IVacancyClient client, IOptions<QualificationsConfiguration> qualificationsConfigOptions)
        {
            _client = client;
            _qualificationsConfig = qualificationsConfigOptions.Value;
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
                QualificationTypes = _qualificationsConfig.QualificationTypes,
                Qualifications = vacancy.Qualifications.ToViewModel()
            };

            return vm;
        }

        public async Task<QualificationsViewModel> GetQualificationsViewModelAsync(QualificationsEditModel m)
        {
            var vm = await GetQualificationsViewModelAsync(m.VacancyId);

            vm.Qualifications = m.Qualifications;

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
            
            vacancy.Qualifications = SortQualifications(qualifications.ToEntity());
            
            await _client.UpdateVacancyAsync(vacancy, false);
        }
        
        private List<Qualification> SortQualifications(IEnumerable<Qualification> qualificationsToSort)
        {
            var weightingComparer = Comparer<QualificationWeighting?>.Create((x, y) =>
            {
                if (x == y)
                {
                    return 0;
                }

                if (x == QualificationWeighting.Essential)
                {
                    return -1;
                }

                return 1;
            });
            
            var qualifications = qualificationsToSort.OrderBy(q => q.Weighting, weightingComparer)
                .ThenBy(q => q.QualificationType).ThenBy(q => q.Subject).ToList();

            return qualifications;
        }
    }
}

