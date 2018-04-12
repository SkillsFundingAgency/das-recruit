using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyManageOrchestrator
    {
        private readonly IVacancyClient _client;
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;

        public VacancyManageOrchestrator(IVacancyClient client, DisplayVacancyViewModelMapper vacancyDisplayMapper)
        {
            _client = client;
            _vacancyDisplayMapper = vacancyDisplayMapper;
        }

        public ManageVacancy GetVacancyDisplayViewModelAsync(Vacancy vacancy)
        {
            switch (vacancy.Status)
            {
                case VacancyStatus.Submitted:
                    var submittedViewModel = new SubmittedVacancyViewModel();
                    _vacancyDisplayMapper.MapFromVacancy(submittedViewModel, vacancy);
                    submittedViewModel.SubmittedDate = vacancy.SubmittedDate.Value.AsDisplayDate();
                    return new ManageVacancy
                    {
                        ViewModel = submittedViewModel,
                        ViewName = ViewNames.ManageSubmittedVacancyView
                    };
                case VacancyStatus.Live:
                    var liveViewModel = new LiveVacancyViewModel();
                    _vacancyDisplayMapper.MapFromVacancy(liveViewModel, vacancy);
                    return new ManageVacancy
                    {
                        ViewModel = liveViewModel,
                        ViewName = ViewNames.ManageLiveVacancyView
                    };
                case VacancyStatus.Closed:
                case VacancyStatus.Referred:
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
