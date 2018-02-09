using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Employer.Web.Services
{
    public interface IGetAssociatedEmployerAccountsService
    {
        string[] GetAssociatedAccounts(string userId);
    }
}
