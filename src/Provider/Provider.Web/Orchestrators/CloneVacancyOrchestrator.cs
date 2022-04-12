using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.CloneVacancy;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using ErrorMessages = Esfa.Recruit.Shared.Web.ViewModels.ErrorMessages;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
	public class CloneVacancyOrchestrator : EntityValidatingOrchestrator<Vacancy, CloneVacancyWithNewDatesEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.StartDateEndDate ;
        public const string ChangeBothDatesTitle = "Change the closing date and start date";
        public const string ChangeEitherDatesTitle = "Change the closing date or start date";
        private readonly IRecruitVacancyClient _vacancyClient;
		private readonly ITimeProvider _timeProvider;
        private readonly IUtility _utility;

        public CloneVacancyOrchestrator(IRecruitVacancyClient vacancyClient, 
            ITimeProvider timeProvider, ILogger<CloneVacancyOrchestrator> logger, IUtility utility) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _utility = utility;
        }

        public async Task<CloneVacancyDatesQuestionViewModel> GetCloneVacancyDatesQuestionViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await GetCloneableAuthorisedVacancyAsync(vrm);

            if (IsNewDatesRequired(vacancy))
                throw new InvalidStateException(string.Format(ErrorMessages.CannotCloneVacancyWithSameDates, vacancy.Title));

            var vm = new CloneVacancyDatesQuestionViewModel 
            {
                StartDate = vacancy.StartDate?.AsGdsDate(),
                ClosingDate = vacancy.ClosingDate?.AsGdsDate()
            };

            return vm;
        }

        public async Task<CloneVacancyWithNewDatesViewModel> GetCloneVacancyWithNewDatesViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await GetCloneableAuthorisedVacancyAsync(vrm);

            var isNewDatesForced = IsNewDatesRequired(vacancy);
            if(isNewDatesForced)
            {
                return new CloneVacancyWithNewDatesViewModel 
                {
                    IsNewDatesForced = isNewDatesForced,
                    Title = ChangeBothDatesTitle,
                };
            }
            else
            {
                return new CloneVacancyWithNewDatesViewModel
                {
                    IsNewDatesForced = isNewDatesForced,
                    Title = ChangeEitherDatesTitle,
                    ClosingDay = $"{vacancy.ClosingDate.Value.Day:00}",
                    ClosingMonth = $"{vacancy.ClosingDate.Value.Month:00}",
                    ClosingYear = $"{vacancy.ClosingDate.Value.Year}",
                    StartDay = $"{vacancy.StartDate.Value.Day:00}",
                    StartMonth = $"{vacancy.StartDate.Value.Month:00}",
                    StartYear = $"{vacancy.StartDate.Value.Year}",
                    CurrentYear = _timeProvider.Now.Year
                };
            }
        }

        public async Task<CloneVacancyWithNewDatesViewModel> GetDirtyCloneVacancyWithNewDatesViewModelAsync(CloneVacancyWithNewDatesEditModel dirtyModel)
        {
            var model = await GetCloneVacancyWithNewDatesViewModelAsync(dirtyModel);

            model.ClosingDay = dirtyModel.ClosingDay;
            model.ClosingMonth = dirtyModel.ClosingMonth;
            model.ClosingYear = dirtyModel.ClosingYear;

            model.StartDay = dirtyModel.StartDay;
            model.StartMonth = dirtyModel.StartMonth;
            model.StartYear = dirtyModel.StartYear;
            model.CurrentYear = _timeProvider.Now.Year;

            return model;
        }

        public bool IsNewDatesRequired(Vacancy vacancy)
            => vacancy.ClosingDate < _timeProvider.Now.Date;

        public async Task<Guid> PostCloneVacancyWithSameDates(CloneVacancyDatesQuestionEditModel model, VacancyUser user)
        {
            var vacancy = await GetCloneableAuthorisedVacancyAsync(model);

            var newVacancyId = await _vacancyClient.CloneVacancyAsync(
                model.VacancyId.GetValueOrDefault(), 
                user, 
                SourceOrigin.ProviderWeb, 
                vacancy.StartDate.GetValueOrDefault(), 
                vacancy.ClosingDate.GetValueOrDefault());
            
            return newVacancyId;
        }

        public async Task<OrchestratorResponse<Guid>> PostCloneVacancyWithNewDates(CloneVacancyWithNewDatesEditModel model, VacancyUser user)
        {
            var vacancy = await GetCloneableAuthorisedVacancyAsync(model);

            var startDate = model.StartDate.AsDateTimeUk()?.ToUniversalTime();
            var closingDate = model.ClosingDate.AsDateTimeUk()?.ToUniversalTime();
            vacancy.StartDate = startDate;
            vacancy.ClosingDate = closingDate;

            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValidationRules),
                v => 
                    _vacancyClient.CloneVacancyAsync(
                        model.VacancyId.GetValueOrDefault(),
                        user,
                        SourceOrigin.ProviderWeb,
                        startDate.Value,
                        closingDate.Value)
                );
        }

        public async Task<Vacancy> GetCloneableAuthorisedVacancyAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

            _utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            if (!vacancy.CanClone)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForCloning, vacancy.Title));

            return vacancy;
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, CloneVacancyWithNewDatesEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, CloneVacancyWithNewDatesEditModel>
            {
                { e => e.StartDate, vm => vm.StartDate },
                { e => e.ClosingDate, vm => vm.ClosingDate }
            };

            return mappings;
        }

    }
}