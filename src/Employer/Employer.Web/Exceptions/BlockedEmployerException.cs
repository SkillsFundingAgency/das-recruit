using System;

namespace Esfa.Recruit.Employer.Web.Exceptions
{
    public class BlockedEmployerException : Exception
    {
        public BlockedEmployerException(string message) : base(message){}
    }
}
