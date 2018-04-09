using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class QualificationsOrchestrator : EntityValidatingOrchestrator<Vacancy, QualificationsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Qualifications;
        private readonly IVacancyClient _client;
        private readonly QualificationsConfiguration _qualificationsConfig;

        public QualificationsOrchestrator(IVacancyClient client, IOptions<QualificationsConfiguration> qualificationsConfigOptions, ILogger<SkillsOrchestrator> logger)
            : base(logger)
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
                Qualifications = vacancy.Qualifications.SortQualifications(_qualificationsConfig.QualificationTypes).ToViewModel()
            };

            return vm;
        }

        public async Task<QualificationsViewModel> GetQualificationsViewModelAsync(QualificationsEditModel m)
        {
            var vm = await GetQualificationsViewModelAsync(m.VacancyId);

            vm.Qualifications = m.Qualifications;
            vm.QualificationType = m.QualificationType;
            vm.Subject = m.Subject;
            vm.Grade = m.Grade;
            vm.Weighting = m.Weighting;

            return vm;
        }

        public async Task<OrchestratorResponse> PostQualificationsEditModelAsync(QualificationsEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var qualifications = (m.Qualifications.ToEntity() ?? new List<Qualification>());
            vacancy.Qualifications = qualifications.SortQualifications(_qualificationsConfig.QualificationTypes);
            m.Qualifications = vacancy.Qualifications.ToViewModel();
            
            return await ValidateAndExecute(vacancy,
                v => 
                {
                    var result = _client.Validate(v, ValidationRules);
                    SyncErrorsAndModel(result.Errors, m);
                    return result;
                },
                v => _client.UpdateVacancyAsync(vacancy)
            );
        }
        
        private List<QualificationEditModel> SortQualifications(List<QualificationEditModel> qualificationsToSort)
        {
            return  qualificationsToSort
                    .OrderBy(q => _qualificationsConfig.QualificationTypes.IndexOf(q.QualificationType))
                    .ThenBy(q => q.Subject)
                    .ToList();
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, QualificationsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, QualificationsEditModel>();

            mappings.Add(e => e.Qualifications, vm => vm.Qualifications);

            return mappings;
        }

        private void SyncErrorsAndModel(IList<EntityValidationError> errors, QualificationsEditModel m)
        {
            //Get the index position for the first invalid qualification
            var qualificationIndex = errors.FirstOrDefault(e => e.PropertyName.StartsWith($"{nameof(m.Qualifications)}["))?.GetIndexPosition();
            if (!qualificationIndex.HasValue)
            {
                return;
            }
            
            //Populate the inputs so we can edit the invalid qualification
            var invalidQualification = m.Qualifications[qualificationIndex.Value];
            m.QualificationType = invalidQualification.QualificationType;
            m.Subject = invalidQualification.Subject;
            m.Grade = invalidQualification.Grade;
            m.Weighting = invalidQualification.Weighting;
            m.Qualifications.Remove(invalidQualification);

            //Get all the errors for the qualification at the index position
            var qualificationErrors = errors.Where(e => e.PropertyName
                .StartsWith($"{nameof(m.Qualifications)}[{qualificationIndex}]"));

            //Attach the errors to the inputs ModelState
            foreach (var error in qualificationErrors)
            {
                error.PropertyName = error.PropertyName.Substring(error.PropertyName.LastIndexOf('.') + 1);
            }
            
            //Remove other qualification errors
            errors.Where(e => e.PropertyName.StartsWith($"{nameof(m.Qualifications)}[")).ToList()
                .ForEach(r => errors.Remove(r));
        }
    }
}

