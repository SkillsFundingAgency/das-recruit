using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface ICreateDashboards
    {
        Task BuildDashboard(string employerAccountId);
    }
}