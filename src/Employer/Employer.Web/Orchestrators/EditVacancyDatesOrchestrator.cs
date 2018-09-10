using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.EditVacancyDates;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Employer.Web.Extensions;
using static Esfa.Recruit.Employer.Web.ViewModels.ValidationMessages;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class EditVacancyDatesOrchestrator : EntityValidatingOrchestrator<Vacancy, EditVacancyDatesEditModel>
    {
        private const VacancyRuleSet ValdationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.TrainingProgramme | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate | VacancyRuleSet.Wage;
        private readonly IEmployerVacancyClient _client;

        private readonly EntityValidationResult _proposedClosingDateMustBeNewerError = new EntityValidationResult
        {
            Errors = new[] 
            { 
                new EntityValidationError((long)VacancyRuleSet.ClosingDate, nameof(EditVacancyDatesEditModel.ClosingDate), ErrorMessages.ChangedClosingDateMustBeNewer, "201") 
            }
        };

        public EditVacancyDatesOrchestrator(IEmployerVacancyClient client, ILogger<EditVacancyDatesOrchestrator> logger) : base(logger)
        {
            _client = client;
        }

        public async Task<Vacancy> GetVacancyAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            return vacancy;
        }

        public Task<OrchestratorResponse<EditVacancyDatesViewModel>> GetEditVacancyDatesViewModel(VacancyRouteModel vrm, string proposedClosingDate, string proposedStartDate)
        {
            var vacancyTask = GetVacancyAsync(vrm);
            var programmesTask = _client.GetActiveApprenticeshipProgrammesAsync();

            Task.WaitAll(vacancyTask, programmesTask);

            var vacancy = vacancyTask.Result;
            var programmes = programmesTask.Result;

            var vm = new EditVacancyDatesViewModel
            {
                ClosingDay = $"{vacancy.ClosingDate.Value.Day:00}",
                ClosingMonth = $"{vacancy.ClosingDate.Value.Month:00}",
                ClosingYear = $"{vacancy.ClosingDate.Value.Year}",

                StartDay = $"{vacancy.StartDate.Value.Day:00}",
                StartMonth = $"{vacancy.StartDate.Value.Month:00}",
                StartYear = $"{vacancy.StartDate.Value.Year}",

                ProgammeName = programmes.First(p => p.Id == vacancy.ProgrammeId).Title
            };

            var resp = new OrchestratorResponse<EditVacancyDatesViewModel>(vm);
            ValidateProposedDateChanges(proposedClosingDate, proposedStartDate, resp);

            return Task.FromResult(resp);
        }

        private void ValidateProposedDateChanges(string proposedClosingDate, string proposedStartDate, OrchestratorResponse<EditVacancyDatesViewModel> resp)
        {
            if (!string.IsNullOrEmpty(proposedClosingDate) || !string.IsNullOrEmpty(proposedStartDate))
            {
                if (proposedClosingDate?.Length > 0)
                {
                    if (DateTime.TryParse(proposedClosingDate, out var parsedClosingDate) == false)
                    {
                        resp.Errors.Errors.Add(new EntityValidationError((long)VacancyRuleSet.ClosingDate, nameof(EditVacancyDatesEditModel.ClosingDate), DateValidationMessages.TypeOfDate.ClosingDate, "202"));
                    }
                    else
                    {
                        resp.Data.ClosingDay = parsedClosingDate.Day.ToString("00");
                        resp.Data.ClosingMonth = parsedClosingDate.Month.ToString("00");
                        resp.Data.ClosingYear = parsedClosingDate.Year.ToString();
                    }
                }

                if (proposedStartDate?.Length > 0)
                {
                    if (DateTime.TryParse(proposedStartDate, out var parsedStartDate) == false)
                    {
                        resp.Errors.Errors.Add(new EntityValidationError((long)VacancyRuleSet.StartDate, nameof(EditVacancyDatesEditModel.StartDate), DateValidationMessages.TypeOfDate.StartDate, "202"));
                    }
                    else
                    {
                        resp.Data.StartDay = parsedStartDate.Day.ToString("00");
                        resp.Data.StartMonth = parsedStartDate.Month.ToString("00");
                        resp.Data.StartYear = parsedStartDate.Year.ToString();
                    }
                }
            }
        }

        public async Task<EditVacancyDatesViewModel> GetEditVacancyDatesViewModel(EditVacancyDatesEditModel m)
        {
            var resp = await GetEditVacancyDatesViewModel(m, m.ClosingDate.ToDateQueryString(), m.StartDate.ToDateQueryString());

            resp.Data.ClosingDay = m.ClosingDay;
            resp.Data.ClosingMonth = m.ClosingMonth;
            resp.Data.ClosingYear = m.ClosingYear;

            resp.Data.StartDay = m.StartDay;
            resp.Data.StartMonth = m.StartMonth;
            resp.Data.StartYear = m.StartYear;

            return resp.Data;
        }

        public async Task<OrchestratorResponse> ValidateEditVacancyDatesViewModel(EditVacancyDatesEditModel m)
        {
            var vacancy = await GetVacancyAsync(m);

            var proposedClosingDate = m.ClosingDate.AsDateTimeUk()?.ToUniversalTime();

            if (proposedClosingDate < vacancy.ClosingDate)
                return new OrchestratorResponse(_proposedClosingDateMustBeNewerError);

            vacancy.ClosingDate = proposedClosingDate;
            vacancy.StartDate = m.StartDate.AsDateTimeUk()?.ToUniversalTime();

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValdationRules),
                _ => Task.CompletedTask
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