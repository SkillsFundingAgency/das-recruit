using Esfa.Recruit.Employer.Web.ViewModels.NewVacancy;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public interface INewVacancyOrchestrator
    {
        IndexViewModel GetIndexViewModel();
        void PostIndexViewModel(IndexViewModel vm);
    }
}