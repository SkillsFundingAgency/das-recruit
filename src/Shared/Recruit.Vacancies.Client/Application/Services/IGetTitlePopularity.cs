using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IGetTitlePopularity
    {
        Task<int> GetTitlePopularityAsync(string larsCode, string title);
    }
}