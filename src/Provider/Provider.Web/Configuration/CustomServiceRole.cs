using SFA.DAS.DfESignIn.Auth.Enums;
using SFA.DAS.DfESignIn.Auth.Interfaces;

namespace Esfa.Recruit.Provider.Web.Configuration;

public class CustomServiceRole : ICustomServiceRole
{
    public string RoleClaimType => ProviderRecruitClaims.DfEUserServiceTypeClaimTypeIdentifier;

    // <inherit-doc/>
    public CustomServiceRoleValueType RoleValueType => CustomServiceRoleValueType.Code;
}