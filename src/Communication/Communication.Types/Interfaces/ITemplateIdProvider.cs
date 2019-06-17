using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface ITemplateIdProvider
    {
        string ProviderName { get; }
        Task<string> GetTemplateId(CommunicationMessage message);
    }
}