using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Employer.Web.Middleware;

public class EmployerAccountOwnerOrTransactorRequirement : IAuthorizationRequirement { }