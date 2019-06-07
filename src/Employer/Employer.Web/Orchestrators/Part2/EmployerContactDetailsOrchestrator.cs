﻿using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class EmployerContactDetailsOrchestrator : EntityValidatingOrchestrator<Vacancy, EmployerContactDetailsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerContactDetails;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public EmployerContactDetailsOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<EmployerContactDetailsOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<EmployerContactDetailsViewModel> GetEmployerContactDetailsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.EmployerContactDetails_Get);

            var vm = new EmployerContactDetailsViewModel
            {
                Title = vacancy.Title,
                EmployerContactName = vacancy.EmployerContact?.Name,
                EmployerContactEmail = vacancy.EmployerContact?.Email,
                EmployerContactPhone = vacancy.EmployerContact?.Phone,
                EmployerName = await _vacancyClient.GetEmployerNameAsync(vacancy)
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetEmployerContactDetailsFieldIndicators());
            }

            return vm;
        }

        public async Task<EmployerContactDetailsViewModel> GetEmployerContactDetailsViewModelAsync(EmployerContactDetailsEditModel m)
        {
            var vm = await GetEmployerContactDetailsViewModelAsync((VacancyRouteModel)m);

            vm.EmployerContactName = m.EmployerContactName;
            vm.EmployerContactEmail = m.EmployerContactEmail;
            vm.EmployerContactPhone = m.EmployerContactPhone;

            return vm;
        }

        public async Task<OrchestratorResponse> PostEmployerContactDetailsEditModelAsync(EmployerContactDetailsEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.EmployerContactDetails_Post);

            vacancy.EmployerContact = new ContactDetail
            {
                Name = m.EmployerContactName,
                Email = m.EmployerContactEmail,
                Phone = m.EmployerContactPhone
            };

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EmployerContactDetailsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerContactDetailsEditModel>();

            mappings.Add(e => e.EmployerContact.Name, vm => vm.EmployerContactName);
            mappings.Add(e => e.EmployerContact.Email, vm => vm.EmployerContactEmail);
            mappings.Add(e => e.EmployerContact.Phone, vm => vm.EmployerContactPhone);

            return mappings;
        }
    }
}
