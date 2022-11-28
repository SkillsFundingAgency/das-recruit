using SFA.DAS.Http.Configuration;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount
{
    public class PasAccountApiConfiguration : IManagedIdentityClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        
        public string IdentifierUri { get; set; }
    }
}
