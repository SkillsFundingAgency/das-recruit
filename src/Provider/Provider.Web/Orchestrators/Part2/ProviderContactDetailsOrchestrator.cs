﻿using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ProviderContactDetails;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class ProviderContactDetailsOrchestrator : VacancyValidatingOrchestrator<ProviderContactDetailsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ProviderContactDetails;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;

        public ProviderContactDetailsOrchestrator(IRecruitVacancyClient vacancyClient, ILogger<ProviderContactDetailsOrchestrator> logger, IReviewSummaryService reviewSummaryService, IUtility utility) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
        }

        public async Task<ProviderContactDetailsViewModel> GetProviderContactDetailsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.ProviderContactDetails_Get);

            var vm = new ProviderContactDetailsViewModel
            {
                Title = vacancy.Title,
                ProviderContactName = vacancy.ProviderContact?.Name,
                ProviderContactEmail = vacancy.ProviderContact?.Email,
                ProviderContactPhone = vacancy.ProviderContact?.Phone,
                ProviderName = vacancy.TrainingProvider?.Name,
                VacancyId = vrm.VacancyId,
                Ukprn = vrm.Ukprn,
                AddContactDetails = !string.IsNullOrEmpty(vacancy.ProviderContact?.Name) || 
                                    !string.IsNullOrEmpty(vacancy.ProviderContact?.Email) ||
                                    !string.IsNullOrEmpty(vacancy.ProviderContact?.Phone) ? true : (bool?) null
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                   ReviewFieldMappingLookups.GetProviderContactDetailsFieldIndicators());
            }

            vm.IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy);

            return vm;
        }

        public async Task<ProviderContactDetailsViewModel> GetProviderContactDetailsViewModelAsync(ProviderContactDetailsEditModel m)
        {
            var vm = await GetProviderContactDetailsViewModelAsync((VacancyRouteModel)m);

            vm.ProviderContactName = m.ProviderContactName;
            vm.ProviderContactEmail = m.ProviderContactEmail;
            vm.ProviderContactPhone = m.ProviderContactPhone;

            return vm;
        }

        public async Task<OrchestratorResponse> PostProviderContactDetailsEditModelAsync(ProviderContactDetailsEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.ProviderContactDetails_Post);
            vacancy.HasChosenProviderContactDetails = true;
            
            if (vacancy.ProviderContact == null)
            {
                vacancy.ProviderContact = new ContactDetail();
            }
                

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.ProviderContact.Name,
                FieldIdResolver.ToFieldId(v => v.ProviderContact.Name),
                vacancy,
                (v) => { return v.ProviderContact.Name = m.ProviderContactName; });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.ProviderContact.Email,
                FieldIdResolver.ToFieldId(v => v.ProviderContact.Email),
                vacancy,
                (v) => { return v.ProviderContact.Email = m.ProviderContactEmail; });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.ProviderContact.Phone,
                FieldIdResolver.ToFieldId(v => v.ProviderContact.Phone),
                vacancy,
                (v) => { return v.ProviderContact.Phone = m.ProviderContactPhone; });

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ProviderContactDetailsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ProviderContactDetailsEditModel>();

            mappings.Add(e => e.ProviderContact.Name, vm => vm.ProviderContactName);
            mappings.Add(e => e.ProviderContact.Email, vm => vm.ProviderContactEmail);
            mappings.Add(e => e.ProviderContact.Phone, vm => vm.ProviderContactPhone);

            return mappings;
        }
    }
}
