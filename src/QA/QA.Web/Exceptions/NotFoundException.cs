using System;

namespace Esfa.Recruit.Qa.Web.Exceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}