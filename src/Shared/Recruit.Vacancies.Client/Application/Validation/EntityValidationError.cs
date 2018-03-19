using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    public class EntityValidationError
    {
        public EntityValidationError(string propertyName, string errorMessage, string errorCode)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
    }
}