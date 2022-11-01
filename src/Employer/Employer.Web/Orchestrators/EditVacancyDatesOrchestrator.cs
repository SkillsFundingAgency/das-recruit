using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.EditVacancyDates;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ErrorMessages;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class EditVacancyDatesOrchestrator : EntityValidatingOrchestrator<Vacancy, EditVacancyDatesEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.TrainingProgramme | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate | VacancyRuleSet.MinimumWage;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ITimeProvider _timeProvider;
        private readonly IUtility _utility;

        private readonly EntityValidationResult _proposedClosingDateMustBeNewerError = new EntityValidationResult
        {
            Errors = new[] 
            { 
                new EntityValidationError((long)VacancyRuleSet.ClosingDate, nameof(EditVacancyDatesEditModel.ClosingDate), ErrorMessages.ChangedClosingDateMustBeNewer, "201") 
            }
        };

        public EditVacancyDatesOrchestrator(IRecruitVacancyClient vacancyClient, ILogger<EditVacancyDatesOrchestrator> logger, ITimeProvider timeProvider, IUtility utility) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _utility = utility;
        }

        public async Task<Vacancy> GetVacancyAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId);

            _utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            if (vacancy.CanExtendStartAndClosingDates == false)
                throw new InvalidStateException(string.Format(ErrMsg.VacancyDatesCannotBeEdited, vacancy.Title));

            return vacancy;
        }

        public async Task<OrchestratorResponse<EditVacancyDatesViewModel>> GetEditVacancyDatesViewModelAsync(VacancyRouteModel vrm, DateTime? proposedClosingDate, DateTime? proposedStartDate)
        {
            var vacancyTask = GetVacancyAsync(vrm);
            var programmesTask = _vacancyClient.GetActiveApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacancyTask, programmesTask);

            var vacancy = vacancyTask.Result;
            var programmes = programmesTask.Result;

            var vm = new EditVacancyDatesViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                ClosingDay = $"{vacancy.ClosingDate.Value.Day:00}",
                ClosingMonth = $"{vacancy.ClosingDate.Value.Month:00}",
                ClosingYear = $"{vacancy.ClosingDate.Value.Year}",

                StartDay = $"{vacancy.StartDate.Value.Day:00}",
                StartMonth = $"{vacancy.StartDate.Value.Month:00}",
                StartYear = $"{vacancy.StartDate.Value.Year}",

                CurrentYear = _timeProvider.Now.Year,

                ProgrammeName = programmes.FirstOrDefault(p => p.Id == vacancy.ProgrammeId)?.Title ?? "",
                
                Title = vacancy.Title
            };

            var resp = new OrchestratorResponse<EditVacancyDatesViewModel>(vm);
            ApplyProposedDateChanges(proposedClosingDate, proposedStartDate, resp);

            return resp;
        }

        private void ApplyProposedDateChanges(DateTime? proposedClosingDate, DateTime? proposedStartDate, OrchestratorResponse<EditVacancyDatesViewModel> resp)
        {
            if (proposedClosingDate.HasValue == false || proposedStartDate.HasValue == false)
                return;

            resp.Data.ClosingDay = proposedClosingDate.Value.Day.ToString("00");
            resp.Data.ClosingMonth = proposedClosingDate.Value.Month.ToString("00");
            resp.Data.ClosingYear = proposedClosingDate.Value.Year.ToString();

            resp.Data.StartDay = proposedStartDate.Value.Day.ToString("00");
            resp.Data.StartMonth = proposedStartDate.Value.Month.ToString("00");
            resp.Data.StartYear = proposedStartDate.Value.Year.ToString();
        }

        public async Task<EditVacancyDatesViewModel> GetEditVacancyDatesViewModelAsync(EditVacancyDatesEditModel m)
        {
            var resp = await GetEditVacancyDatesViewModelAsync(m, null, null);

            resp.Data.ClosingDay = m.ClosingDay;
            resp.Data.ClosingMonth = m.ClosingMonth;
            resp.Data.ClosingYear = m.ClosingYear;

            resp.Data.StartDay = m.StartDay;
            resp.Data.StartMonth = m.StartMonth;
            resp.Data.StartYear = m.StartYear;
            resp.Data.CurrentYear = _timeProvider.Now.Year;

            return resp.Data;
        }

        public async Task<OrchestratorResponse> PostEditVacancyDatesEditModelAsync(EditVacancyDatesEditModel m, VacancyUser user)
        {
            var vacancy = await GetVacancyAsync(m);

            var proposedClosingDate = m.ClosingDate.AsDateTimeUk()?.ToUniversalTime();

            if (proposedClosingDate < vacancy.ClosingDate)
                return new OrchestratorResponse(_proposedClosingDateMustBeNewerError);

            var proposedVacancyStartDate = m.StartDate.AsDateTimeUk()?.ToUniversalTime();
            
            vacancy.ClosingDate = proposedClosingDate;
            vacancy.StartDate = proposedVacancyStartDate;
            var updateKind = VacancyHelper.DetermineLiveUpdateKind(vacancy, proposedClosingDate, proposedVacancyStartDate);

            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdatePublishedVacancyAsync(vacancy, user, updateKind)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EditVacancyDatesEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EditVacancyDatesEditModel>
            {
                { e => e.StartDate, vm => vm.StartDate },
                { e => e.ClosingDate, vm => vm.ClosingDate }
            };

            return mappings;
        }
    }
}