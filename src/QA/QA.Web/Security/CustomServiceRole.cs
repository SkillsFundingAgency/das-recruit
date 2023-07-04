using SFA.DAS.DfESignIn.Auth.Constants;
using SFA.DAS.DfESignIn.Auth.Enums;
using SFA.DAS.DfESignIn.Auth.Interfaces;

namespace Esfa.Recruit.Qa.Web.Security
{
    /// <summary>
    /// Class to define the Custom Service Role used in DfESignIn Authentication Service.
    /// </summary>
    public class CustomServiceRole : ICustomServiceRole
    {
        public string RoleClaimType
        {
            get
            {
                return CustomClaimsIdentity.Service;
            }
        }

        public CustomServiceRoleValueType RoleValueType
        {
            get
            {
                return CustomServiceRoleValueType.Name;
            }
        }
    }
}
