using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class QualificationsOrchestrator : EntityValidatingOrchestrator<Vacancy, QualificationEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Qualifications;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        
        public QualificationsOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<QualificationsOrchestrator> logger, IReviewSummaryService reviewSummaryService)
            : base(logger)
        {
            _client = client;
            _reviewSummaryService = reviewSummaryService;
            _vacancyClient = vacancyClient;
        }

        public async Task<QualificationsViewModel> GetQualificationsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Qualifications_Get);

            var qualifications = vacancy.Qualifications ?? new List<Qualification>();

            var vm = new QualificationsViewModel
            {
                Title = vacancy.Title,
                Qualifications = qualifications.Select(q => new QualificationEditModel
                { 
                    Subject = q.Subject,
                    QualificationType = q.QualificationType,
                    Weighting = q.Weighting,
                    Grade = q.Grade
                }).ToList()
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                   ReviewFieldMappingLookups.GetQualificationsFieldIndicators());
            }

            return vm;
        }

        public async Task<AddQualificationViewModel> GetAddQualificationViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Qualification_Add_Get);
            var allQualificationsTask = _vacancyClient.GetCandidateQualificationsAsync();

            await Task.WhenAll(vacancyTask, allQualificationsTask);

            var vm = new AddQualificationViewModel
            {
                Title = vacancyTask.Result.Title,
                QualificationTypes = allQualificationsTask.Result,
            };

            return vm;
        }

        public async Task<AddQualificationViewModel> GetAddQualificationViewModelAsync(VacancyRouteModel vrm, QualificationEditModel m)
        {
            var vm = await GetAddQualificationViewModelAsync(vrm);

            vm.QualificationType = m.QualificationType;
            vm.Subject = m.Subject;
            vm.Grade = m.Grade;
            vm.Weighting = m.Weighting;

            return vm;
        }

        public async Task<OrchestratorResponse> PostAddQualificationEditModelAsync(VacancyRouteModel vrm, QualificationEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Qualification_Add_Post);

            if (vacancy.Qualifications == null)
                vacancy.Qualifications = new List<Qualification>();

            var qualification = new Qualification
            {
                QualificationType = m.QualificationType,
                Grade = m.Grade,
                Subject = m.Subject,
                Weighting = m.Weighting
            };

            vacancy.Qualifications.Add(qualification);

            var allQualifications = await _vacancyClient.GetCandidateQualificationsAsync();
            vacancy.Qualifications = vacancy.Qualifications.SortQualifications(allQualifications).ToList();

            return await ValidateAndExecute(vacancy,
                v => 
                {
                    var result = _vacancyClient.ValidateQualification(qualification);
                    SyncErrorsAndModel(result.Errors, m);
                    return result;
                },
                v => _vacancyClient.UpdateDraftVacancyAsync(v, user));
        }

        public async Task DeleteQualificationAsync(VacancyRouteModel vrm, int index, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Qualifications_Get);

            if (vacancy.Qualifications == null)
                return;

            vacancy.Qualifications.RemoveAt(index);

            await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
        }

        private void SyncErrorsAndModel(IList<EntityValidationError> errors, QualificationEditModel m)
        {
            var qualificationPropertyName = nameof(Vacancy.Qualifications);

            //Get the index position for the first invalid qualification
            var qualificationIndex = errors.FirstOrDefault(e => e.PropertyName.StartsWith($"{qualificationPropertyName}["))?.GetIndexPosition();
            if (!qualificationIndex.HasValue)
            {
                return;
            }
            
            //Get all the errors for the qualification at the index position
            var qualificationErrors = errors.Where(e => e.PropertyName
                .StartsWith($"{qualificationPropertyName}[{qualificationIndex}]"));

            //Attach the errors to the inputs ModelState
            foreach (var error in qualificationErrors)
            {
                error.PropertyName = error.PropertyName.Substring(error.PropertyName.LastIndexOf('.') + 1);
            }

            //Remove other qualification errors
            errors.Where(e => e.PropertyName.StartsWith($"{qualificationPropertyName}[")).ToList()
                .ForEach(r => errors.Remove(r));
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, QualificationEditModel> DefineMappings()
        {
            return null;
        }
    }
}

