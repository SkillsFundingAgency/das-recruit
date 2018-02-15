using Esfa.Recruit.Employer.Web.ViewModels.NewVacancy;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public interface INewVacancyOrchestrator
    {
        IndexViewModel GetIndexViewModel();
        Task PostIndexViewModelAsync(IndexViewModel vm);
    }
}