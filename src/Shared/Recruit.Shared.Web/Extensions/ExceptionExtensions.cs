using System;
using System.Text;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFormattedExceptionData(this Exception exception)
        {
            var exceptionDataBuilder = new StringBuilder();
            if (exception.Data.Keys.Count > 0) 
            {
                exceptionDataBuilder.Append(" Exception data:");
                foreach(var key in exception.Data.Keys)
                {
                    exceptionDataBuilder.Append($" {key}: {exception.Data[key]}");
                }
            }
            return exceptionDataBuilder.ToString();
        }
    }
}