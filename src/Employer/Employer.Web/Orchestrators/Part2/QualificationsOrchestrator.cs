using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class QualificationsOrchestrator : VacancyValidatingOrchestrator<QualificationEditModel>
    {
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;
        private readonly IFeature _feature;

        public QualificationsOrchestrator(IRecruitVacancyClient vacancyClient, ILogger<QualificationsOrchestrator> logger, IReviewSummaryService reviewSummaryService, IUtility utility, IFeature feature)
            : base(logger)
        {
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
            _feature = feature;
            _vacancyClient = vacancyClient;
        }

        public async Task<QualificationsViewModel> GetQualificationsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Qualifications_Get);

            var qualifications = vacancy.Qualifications ?? new List<Qualification>();

            var vm = new QualificationsViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                Title = vacancy.Title,
                AddQualificationRequirement = vacancy.HasOptedToAddQualifications,
                Qualifications = qualifications.Select(q => new QualificationEditModel
                {
                    Subject = q.Subject,
                    QualificationType = q.QualificationType,
                    Weighting = q.Weighting,
                    Grade = q.Grade,
                    Level = q.Level,
                    OtherQualificationName = q.OtherQualificationName
                }).ToList(),
                IsFaaV2Enabled = _feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements)
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetQualificationsFieldIndicators());
            }
            vm.IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy);
            
            return vm;
        }

        public async Task<OrchestratorResponse> PostAddQualificationEditModel(AddQualificationsEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.Qualifications_Get);
            return await UpdateVacancyWithAddQualifications(vacancy, m, user);
        }

        public async Task<QualificationViewModel> GetQualificationViewModelForAddAsync(VacancyRouteModel vrm)
        {
            var vacancyTask = _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Qualification_Add_Get);
            var allQualificationsTask = _vacancyClient.GetCandidateQualificationsAsync();

            await Task.WhenAll(vacancyTask, allQualificationsTask);

            var vm = GetQualificationViewModel(vacancyTask.Result, allQualificationsTask.Result);
            vm.PostRoute = RouteNames.Qualification_Add_Post;

            return vm;
        }

        public async Task<QualificationViewModel> GetQualificationViewModelForEditAsync(VacancyRouteModel vrm, int index)
        {
            var vacancyTask = _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Qualification_Edit_Get);
            var allQualificationsTask = _vacancyClient.GetCandidateQualificationsAsync();

            await Task.WhenAll(vacancyTask, allQualificationsTask);

            var vacancy = vacancyTask.Result;

            var vm = GetQualificationViewModel(vacancy, allQualificationsTask.Result);

            ValidateIndex(index, vacancy.Qualifications);

            var qualificationToEdit = vacancy.Qualifications[index];

            vm.Index = index;
            var qualificationNameType = _feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements) ? MapToV2Qualification(qualificationToEdit.QualificationType) : qualificationToEdit.QualificationType;
            vm.QualificationType = qualificationNameType;
            vm.Subject = qualificationToEdit.Subject;
            vm.Grade = qualificationToEdit.Grade;
            vm.Weighting = qualificationToEdit.Weighting;
            vm.Level = qualificationToEdit.Level;
            vm.OtherQualificationName = _feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements) && qualificationNameType.Equals("Other") && string.IsNullOrEmpty(qualificationToEdit.OtherQualificationName)
                ? qualificationToEdit.QualificationType : qualificationToEdit.OtherQualificationName;
            vm.PostRoute = RouteNames.Qualification_Edit_Post;
            
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
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Qualification_Add_Post);

            if (vacancy.Qualifications == null)
                vacancy.Qualifications = new List<Qualification>();

            var qualification = new Qualification();
            vacancy.Qualifications.Add(qualification);

            return await UpdateVacancyWithQualificationAsync(vacancy, null, qualification, m, user);
        }

        public async Task<OrchestratorResponse> PostQualificationEditModelForEditAsync(VacancyRouteModel vrm, QualificationEditModel m, VacancyUser user, int index)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Qualification_Edit_Post);

            var qualification = vacancy.Qualifications[index];
            var currentQualification = new Qualification
            {
                QualificationType = qualification.QualificationType,
                Grade = qualification.Grade,
                Subject = qualification.Subject,
                Weighting = qualification.Weighting,
                Level = qualification.Level,
                OtherQualificationName = qualification.OtherQualificationName
            };

            return await UpdateVacancyWithQualificationAsync(vacancy, currentQualification, qualification, m, user);
        }

        public async Task DeleteQualificationAsync(VacancyRouteModel vrm, int index, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Qualifications_Get);

            if (vacancy.Qualifications == null)
                return;

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.Qualifications[index],
                FieldIdResolver.ToFieldId(v => v.Qualifications),
                vacancy,
                (v) =>
                {
                    vacancy.Qualifications.RemoveAt(index);
                    return null;
                });

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
            var cancelRoute = RouteNames.Qualifications_Get;
            var backRoute = RouteNames.Qualifications_Get;
            
            if (vacancy.Qualifications?.Any() == null || !vacancy.Qualifications.Any())
            {
                cancelRoute = RouteNames.Dashboard_Get;
                backRoute = RouteNames.Skills_Get;
            }

            var vm = new QualificationViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                Title = vacancy.Title,
                QualificationTypes = allQualifications,
                CancelRoute = cancelRoute,
                BackRoute = backRoute,
                IsFaaV2Enabled = _feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements)
            };

            foreach (var qualification in allQualifications)
            {
                var q = new QualificationViewModel.Qualification
                {
                    Name = qualification,
                    Data = ""
                };

                if (_feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements))
                {
                    if (qualification == "BTEC")
                    {
                        q.Data = "conditional-btec";
                    }

                    if (qualification == "Other")
                    {
                        q.Data = "conditional-other";
                    }
                }
                
                vm.Qualifications.Add(q);
            }

            return vm;
        }

        private void SetQualificationViewModelFromEditModel(QualificationViewModel vm, QualificationEditModel m)
        {
            vm.QualificationType = m.QualificationType;
            vm.Subject = m.Subject;
            vm.Grade = m.Grade;
            vm.Weighting = m.Weighting;
            vm.Level = m.Level;
        }

        private async Task<OrchestratorResponse> UpdateVacancyWithQualificationAsync(Vacancy vacancy, Qualification currentQualification, Qualification qualification, QualificationEditModel m, VacancyUser user)
        {
            SetVacancyWithEmployerReviewFieldIndicators(
                currentQualification,
                FieldIdResolver.ToFieldId(v => v.Qualifications),
                vacancy,
                (v) => {
                    qualification.QualificationType = m.QualificationType;
                    qualification.Grade = m.Grade;
                    qualification.Subject = m.Subject;
                    qualification.Weighting = m.Weighting;
                    qualification.Level = m.Level;
                    qualification.OtherQualificationName = m.OtherQualificationName;
                    return qualification;
                });

            var qualificationTypes = await _vacancyClient.GetCandidateQualificationsAsync();
            vacancy.Qualifications = vacancy.Qualifications.SortQualifications(qualificationTypes).ToList();

            return await ValidateAndExecute(vacancy,
                v =>
                {
                    var result = _vacancyClient.ValidateQualification(qualification);
                    SyncErrorsAndModel(result.Errors, m);
                    return result;
                },
                v => _vacancyClient.UpdateDraftVacancyAsync(v, user));
        }

        private async Task<OrchestratorResponse> UpdateVacancyWithAddQualifications(Vacancy vacancy,
            AddQualificationsEditModel m, VacancyUser user)
        {
            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.HasOptedToAddQualifications,
                FieldIdResolver.ToFieldId(v => v.HasOptedToAddQualifications),
                vacancy,
                (v) => { return v.HasOptedToAddQualifications = m.AddQualificationRequirement; });

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, VacancyRuleSet.None),
                v => _vacancyClient.UpdateDraftVacancyAsync(v, user));
        }

        private static string MapToV2Qualification(string qualification)
        {
            return qualification switch
            {
                "GCSE or equivalent" => "GCSE",
                "GCSE" => "GCSE",
                "A Level or equivalent" => "A Level",
                "A Level" => "A Level",
                "T Level" => "T Level",
                "BTEC" => "BTEC",
                "Degree" => "Degree",
                "BTEC or equivalent" => "BTEC",
                _ => "Other"
            };
        }
        
        protected override EntityToViewModelPropertyMappings<Vacancy, QualificationEditModel> DefineMappings()
        {
            return null;
        }
    }
}