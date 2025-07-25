﻿using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Mappings.Extensions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Proivder.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Models;
using ApplicationReviewViewModel = Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview.ApplicationReviewViewModel;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public interface IApplicationReviewOrchestrator
    {
        Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewRouteModel rm);
        Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewEditModel m);
        Task<ApplicationReviewStatusChangeInfo> PostApplicationReviewStatusChangeModelAsync(ApplicationReviewStatusChangeModel m, VacancyUser user);
        Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewStatusConfirmationEditModel applicationReviewStatusConfirmationEditModel);
        Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewEditModel rm);
        Task<string> GetApplicationReviewFeedbackViewModelAsync(ApplicationReviewFeedbackViewModel applicationReviewFeedbackViewModel);
        Task<ApplicationReviewFeedbackViewModel> GetApplicationReviewFeedbackViewModelAsync(ApplicationReviewEditModel rm);
    }

    public class ApplicationReviewOrchestrator : IApplicationReviewOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IUtility _utility;

        public ApplicationReviewOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, IUtility utility)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _utility = utility;
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewRouteModel rm)
        {
            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(rm);

            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId.Value);

            if (applicationReview.IsWithdrawn)
                throw new ApplicationWithdrawnException($"Application has been withdrawn. ApplicationReviewId:{applicationReview.Id}", rm.VacancyId.Value);
            var viewModel = applicationReview.ToViewModel();
            viewModel.Ukprn = rm.Ukprn;
            viewModel.VacancyId = rm.VacancyId;
            viewModel.ApplicationReviewId = rm.ApplicationReviewId;
            viewModel.CandidateFeedback = applicationReview.CandidateFeedback;
            viewModel.EmployerFeedback = applicationReview.EmployerFeedback;
            viewModel.VacancyTitle = vacancy.Title;
            viewModel.IsFoundation = vacancy.ApprenticeshipType == ApprenticeshipTypes.Foundation;
            viewModel.CandidateAppliedLocations = applicationReview.Application.CandidateAppliedLocations;

            return viewModel;
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewEditModel m)
        {
            var vm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel)m);

            vm.Outcome = m.Outcome;
            vm.CandidateFeedback = m.CandidateFeedback;

            return vm;
        }

        public async Task<ApplicationReviewStatusChangeInfo> PostApplicationReviewStatusChangeModelAsync(ApplicationReviewStatusChangeModel m, VacancyUser user)
        {
            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(m);

            var shouldMakeOthersUnsuccessful = await _client.SetApplicationReviewStatus(applicationReview.Id, m.Outcome, m.CandidateFeedback, user);

            var applicationReviewStatusChangeInfo = new ApplicationReviewStatusChangeInfo
            {
                ShouldMakeOthersUnsuccessful = shouldMakeOthersUnsuccessful,
                CandidateName = applicationReview.Application.FullName
            };

            return applicationReviewStatusChangeInfo;
        }

        public async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewStatusConfirmationEditModel applicationReviewStatusConfirmationEditModel)
        {
            await _utility.GetAuthorisedApplicationReviewAsync(applicationReviewStatusConfirmationEditModel);

            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(applicationReviewStatusConfirmationEditModel);

            return new ApplicationStatusConfirmationViewModel
            {
                CandidateFeedback = applicationReviewStatusConfirmationEditModel.CandidateFeedback,
                Outcome = applicationReviewStatusConfirmationEditModel.Outcome,
                ApplicationReviewId = applicationReviewStatusConfirmationEditModel.ApplicationReviewId,
                Name = applicationReview.Application.FullName
            };
        }
        public async Task<string> GetApplicationReviewFeedbackViewModelAsync(ApplicationReviewFeedbackViewModel applicationReviewFeedbackViewModel)
        {
            await _utility.GetAuthorisedApplicationReviewAsync(applicationReviewFeedbackViewModel);

            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(applicationReviewFeedbackViewModel);

            return applicationReview.Application.FullName;
        }

        public async Task<ApplicationReviewFeedbackViewModel> GetApplicationReviewFeedbackViewModelAsync(ApplicationReviewEditModel rm)
        {
            var applicationReviewVm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel)rm);

            return new ApplicationReviewFeedbackViewModel
            {
                CandidateFeedback = rm.CandidateFeedback,
                Outcome = rm.Outcome,
                ApplicationReviewId = rm.ApplicationReviewId,
                Name = applicationReviewVm.Name,
                Ukprn = rm.Ukprn,
                VacancyId = rm.VacancyId
            };
        }

        public async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewEditModel rm)
        {
            var applicationReviewVm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel)rm);

            return new ApplicationStatusConfirmationViewModel
            {
                CandidateFeedback = rm.CandidateFeedback,
                FriendlyId = applicationReviewVm.FriendlyId,
                Status = applicationReviewVm.Status,
                Outcome = rm.Outcome,
                ApplicationReviewId = rm.ApplicationReviewId,
                Name = applicationReviewVm.Name,
                Ukprn = rm.Ukprn,
                VacancyId = rm.VacancyId
            };
        }
    }
}
