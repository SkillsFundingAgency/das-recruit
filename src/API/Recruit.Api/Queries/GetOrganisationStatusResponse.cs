using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetOrganisationStatusResponse : ResponseBase
    {
    }

    public class OrganisationStatus
    {
        public string Status { get; }

        public OrganisationStatus(string status)
        {
            Status = status;
        }
    }
}