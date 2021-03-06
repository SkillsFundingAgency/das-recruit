﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class QualificationsOrchestrator : EntityValidatingOrchestrator<Vacancy, QualificationEditModel>
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        
        public QualificationsOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<QualificationsOrchestrator> logger, IReviewSummaryService reviewSummaryService)
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

        public async Task<QualificationViewModel> GetQualificationViewModelForAddAsync(VacancyRouteModel vrm)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Qualification_Add_Get);
            var allQualificationsTask = _vacancyClient.GetCandidateQualificationsAsync();

            await Task.WhenAll(vacancyTask, allQualificationsTask);

            var vm = GetQualificationViewModel(vacancyTask.Result, allQualificationsTask.Result);

            return vm;
        }

        public async Task<QualificationViewModel> GetQualificationViewModelForEditAsync(VacancyRouteModel vrm, int index)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Qualification_Edit_Get);
            var allQualificationsTask = _vacancyClient.GetCandidateQualificationsAsync();

            await Task.WhenAll(vacancyTask, allQualificationsTask);

            var vacancy = vacancyTask.Result;

            var vm = GetQualificationViewModel(vacancy, allQualificationsTask.Result);

            ValidateIndex(index, vacancy.Qualifications);

            var qualificationToEdit = vacancy.Qualifications[index];

            vm.QualificationType = qualificationToEdit.QualificationType;
            vm.Subject = qualificationToEdit.Subject;
            vm.Grade = qualificationToEdit.Grade;
            vm.Weighting = qualificationToEdit.Weighting;
            
            return vm;
        }

        public async Task<QualificationViewModel> GetQualificationViewModelForAddAsync(VacancyRouteModel vrm, QualificationEditModel m)
        {
            var vm = await GetQualificationViewModelForAddAsync(vrm);

            SetQualificationViewModelFromEditModel(vm, m);

            return vm;
        }

        public async Task<QualificationViewModel> GetQualificationViewModelForEditAsync(VacancyRouteModel vrm, QualificationEditModel m, int index)
        {
            var vm = await GetQualificationViewModelForEditAsync(vrm, index);

            SetQualificationViewModelFromEditModel(vm, m);

            return vm;
        }

        public async Task<OrchestratorResponse> PostQualificationEditModelForAddAsync(VacancyRouteModel vrm, QualificationEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Qualification_Add_Post);

            if (vacancy.Qualifications == null)
                vacancy.Qualifications = new List<Qualification>();

            var  qualification = new Qualification();
            vacancy.Qualifications.Add(qualification);

            return await UpdateVacancyWithQualificationAsync(vacancy, qualification, m, user);
        }

        public async Task<OrchestratorResponse> PostQualificationEditModelForEditAsync(VacancyRouteModel vrm, QualificationEditModel m, VacancyUser user, int index)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Qualification_Edit_Post);

            var qualification = vacancy.Qualifications[index];

            return await UpdateVacancyWithQualificationAsync(vacancy, qualification, m, user);
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

        private int ValidateIndex(int index, IEnumerable<Qualification> vacancyQualifications)
        {
            if (index >= 0 && index < vacancyQualifications.Count())
                return index;

            throw new ArgumentException($"Invalid qualification index: {index}");
        }

        private QualificationViewModel GetQualificationViewModel(Vacancy vacancy, IList<string> allQualifications)
        {
            var vm = new QualificationViewModel
            {
                Title = vacancy.Title,
                QualificationTypes = allQualifications,
                CancelRoute = vacancy.Qualifications?.Any() == true ? RouteNames.Qualifications_Get : RouteNames.Vacancy_Preview_Get
            };

            return vm;
        }

        private void SetQualificationViewModelFromEditModel(QualificationViewModel vm, QualificationEditModel m)
        {
            vm.QualificationType = m.QualificationType;
            vm.Subject = m.Subject;
            vm.Grade = m.Grade;
            vm.Weighting = m.Weighting;
        }

        private async Task<OrchestratorResponse> UpdateVacancyWithQualificationAsync(Vacancy vacancy, Qualification qualification, QualificationEditModel m, VacancyUser user)
        {
            qualification.QualificationType = m.QualificationType;
            qualification.Grade = m.Grade;
            qualification.Subject = m.Subject;
            qualification.Weighting = m.Weighting;

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

        protected override EntityToViewModelPropertyMappings<Vacancy, QualificationEditModel> DefineMappings()
        {
            return null;
        }
    }
}