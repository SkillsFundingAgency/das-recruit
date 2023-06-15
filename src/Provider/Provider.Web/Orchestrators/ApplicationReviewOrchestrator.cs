using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Mappings.Extensions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Proivder.Web.Exceptions;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class ApplicationReviewOrchestrator
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

            if (applicationReview.IsWithdrawn)
                throw new ApplicationWithdrawnException($"Application has been withdrawn. ApplicationReviewId:{applicationReview.Id}", rm.VacancyId.Value);
            var viewModel = applicationReview.ToViewModel();
            viewModel.Ukprn = rm.Ukprn;
            viewModel.VacancyId = rm.VacancyId;
            viewModel.ApplicationReviewId = rm.ApplicationReviewId;
            return viewModel;
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewEditModel m)
        {
            var vm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel) m);

            vm.Outcome = m.Outcome;
            vm.CandidateFeedback = m.CandidateFeedback;

            return vm;
        }

        public async Task<string> PostApplicationReviewStatusChangeModelAsync(ApplicationReviewStatusChangeModel m, VacancyUser user)
        {
            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(m);

            switch (m.Outcome.Value)
            {
                case ApplicationReviewStatus.InReview:
                    await _client.SetApplicationReviewToInReview(applicationReview.Id, user);
                    break;
                case ApplicationReviewStatus.Successful:
                    await _client.SetApplicationReviewSuccessful(applicationReview.Id, user);
                    break;
                case ApplicationReviewStatus.Unsuccessful:
                    await _client.SetApplicationReviewUnsuccessful(applicationReview.Id, m.CandidateFeedback, user);
                    break;
                default:
                    throw new ArgumentException("Unhandled ApplicationReviewStatus");
            }
            return applicationReview.Application.FullName;
        }

        internal async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewStatusConfirmationEditModel applicationReviewStatusConfirmationEditModel)
        {
            await _utility.GetAuthorisedApplicationReviewAsync(applicationReviewStatusConfirmationEditModel);

            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(applicationReviewStatusConfirmationEditModel);

            return new ApplicationStatusConfirmationViewModel {
                CandidateFeedback = applicationReviewStatusConfirmationEditModel.CandidateFeedback,
                Outcome = applicationReviewStatusConfirmationEditModel.Outcome,
                ApplicationReviewId = applicationReviewStatusConfirmationEditModel.ApplicationReviewId,
                Name = applicationReview.Application.FullName
            };
        }

        public async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewEditModel rm)
        {
            var applicationReviewVm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel)rm);

            return new ApplicationStatusConfirmationViewModel {
                CandidateFeedback = rm.CandidateFeedback,
                Outcome = rm.Outcome,
                ApplicationReviewId = rm.ApplicationReviewId,
                Name = applicationReviewVm.Name,
                Ukprn = rm.Ukprn,
                VacancyId = rm.VacancyId
            };
        }
    }
}
