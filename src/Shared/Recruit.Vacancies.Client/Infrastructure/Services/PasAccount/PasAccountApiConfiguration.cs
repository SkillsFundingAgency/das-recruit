using SFA.DAS.Http.Configuration;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount
{
    public class PasAccountApiConfiguration : IAzureActiveDirectoryClientConfiguration
    {
        public string ApiBaseUrl { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string IdentifierUri { get; set; }

        public string Tenant { get; set; }
    }
}
