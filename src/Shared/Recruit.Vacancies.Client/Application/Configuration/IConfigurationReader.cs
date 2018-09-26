using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Configuration
{
    public interface IConfigurationReader
    {
        Task<T> GetAsync<T>(string id) where T : class;
    }
}