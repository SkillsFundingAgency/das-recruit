using Esfa.Recruit.Employer.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class StubEmployerAccountService : IEmployerAccountService
    {
        public Task<IDictionary<string, EmployerIdentifier>> GetEmployerIdentifiersAsync(string userId)
        {
            return Task.FromResult<IDictionary<string, EmployerIdentifier>>(new Dictionary<string, EmployerIdentifier>()
                                    {
                                        { "abc", new EmployerIdentifier { AccountId = "abc", EmployerName = "Ozzy Scott" } },
                                        { "MYJR4X", new EmployerIdentifier { AccountId = "MYJR4X", EmployerName = "Mister G" } },
                                        { "MB6YDY", new EmployerIdentifier { AccountId = "MB6YDY", EmployerName = "Dundee" } },
                                        { "MXD78R", new EmployerIdentifier { AccountId = "MXD78R", EmployerName = "TimNiceButDim Ltd" } },
                                    });
        }
    }
}